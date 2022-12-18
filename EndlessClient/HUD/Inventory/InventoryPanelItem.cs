using EndlessClient.Audio;
using EndlessClient.Dialogs;
using EndlessClient.HUD.Panels;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO.Extensions;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using Optional;
using System;
using System.Diagnostics.Tracing;
using XNAControls;

namespace EndlessClient.HUD.Inventory
{
    public class InventoryPanelItem : XNAControl
    {
        public class ItemDragCompletedEventArgs
        {
            public bool ContinueDrag { get; set; } = false;

            public bool RestoreOriginalSlot { get; set; } = false;

            public EIFRecord Data { get; }

            public ItemDragCompletedEventArgs(EIFRecord data) => Data = data;
        }

        // uses absolute coordinates
        private static readonly Rectangle InventoryGridArea = new Rectangle(114, 338, 363, 102);

        private readonly InventoryPanel _inventoryPanel;
        private readonly IActiveDialogProvider _activeDialogProvider;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly Texture2D _itemGraphic;
        private readonly Texture2D _highlightBackground;
        private readonly XNALabel _nameLabel;

        private int _slot;

        private Option<Vector2> _highlightDrawPosition;

        // Ru Paul's drag properties
        // _beingDragged: true when a drag is in progress (either single click or click + drag
        // _followMouse: true when a single-click drag is in progress and the icon should follow the mouse
        //               otherwise, drag event handles following the mouse
        // _oldOffset: the top-left of the parent inventory panel when the drag started
        // _dragPositioned: flag indicating positions are correctly set (prevents icon from flashing in top-left on first update/draw)
        private bool _beingDragged, _followMouse;
        private Vector2 _oldOffset;
        private bool _dragPositioned;

        public int Slot
        {
            get => _slot;
            set
            {
                _slot = value;
                DrawPosition = GetPosition(_slot);
                UpdateNameLabelPosition();
            }
        }

        public InventoryItem InventoryItem { get; set; }

        public string Text
        {
            get => _nameLabel.Text;
            set
            {
                _nameLabel.Text = value;
                _nameLabel.ResizeBasedOnText(16, 9);
                UpdateNameLabelPosition();
            }
        }

        public bool IsDragging => _beingDragged;

        public EIFRecord Data { get; }

        public event EventHandler<EIFRecord> DoubleClick;
        public event EventHandler<ItemDragCompletedEventArgs> DoneDragging;

        public override Rectangle EventArea => IsDragging ? DrawArea : DrawAreaWithParentOffset;

        public InventoryPanelItem(IItemNameColorService itemNameColorService,
                                  InventoryPanel inventoryPanel,
                                  IActiveDialogProvider activeDialogProvider,
                                  ISfxPlayer sfxPlayer,
                                  int slot,
                                  InventoryItem inventoryItem,
                                  EIFRecord data)
        {
            _inventoryPanel = inventoryPanel;
            _activeDialogProvider = activeDialogProvider;
            _sfxPlayer = sfxPlayer;

            Slot = slot;
            InventoryItem = inventoryItem;
            Data = data;

            _itemGraphic = inventoryPanel.NativeGraphicsManager.TextureFromResource(GFXTypes.Items, 2 * data.Graphic, transparent: true);
            _highlightBackground = new Texture2D(Game.GraphicsDevice, 1, 1);
            _highlightBackground.SetData(new[] { Color.FromNonPremultiplied(200, 200, 200, 60) });

            _nameLabel = new XNALabel(Constants.FontSize08)
            {
                Visible = false,
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleCenter,
                ForeColor = itemNameColorService.GetColorForInventoryDisplay(Data),
                BackColor = Color.FromNonPremultiplied(30, 30, 30, 160),
                Text = string.Empty
            };

            OnMouseEnter += (_, _) => _nameLabel.Visible = _inventoryPanel.NoItemsDragging() && _activeDialogProvider.PaperdollDialog.Match(d => d.NoItemsDragging(), () => true);
            OnMouseOver += InventoryPanelItem_OnMouseOver;
            OnMouseLeave += (_, _) => { _nameLabel.Visible = false; _highlightDrawPosition = Option.None<Vector2>(); };

            var (slotWidth, slotHeight) = Data.Size.GetDimensions();
            SetSize(slotWidth * 26 - 3, slotHeight * 26 - 3);
        }

        public int GetCurrentSlotBasedOnPosition()
        {
            if (!_beingDragged)
                return Slot;

            return (int)((DrawPosition.X - _oldOffset.X) / 26) + InventoryPanel.InventoryRowSlots * (int)((DrawPosition.Y - _oldOffset.Y) / 26);
        }

        public void StartDragging()
        {
            if (!_inventoryPanel.NoItemsDragging())
                return;

            _beingDragged = true;
            _nameLabel.Visible = false;

            _oldOffset = ImmediateParent.DrawPositionWithParentOffset;
            DrawPosition = MouseExtended.GetState().Position.ToVector2();

            _sfxPlayer.PlaySfx(SoundEffectID.InventoryPickup);
            DrawOrder = 1000;
        }

        public override void Initialize()
        {
            _nameLabel.Initialize();
            _nameLabel.SetParentControl(_inventoryPanel);
            _nameLabel.ResizeBasedOnText(16, 9);

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_followMouse)
            {
                DrawPosition = MouseExtended.GetState().Position.ToVector2() - new Vector2(DrawArea.Width / 2, DrawArea.Height / 2);
            }

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            _spriteBatch.Begin();

            _highlightDrawPosition.MatchSome(drawPosition =>
            {
                if (InventoryGridArea.Contains(DrawArea.WithPosition(drawPosition)))
                    _spriteBatch.Draw(_highlightBackground, DrawArea.WithPosition(drawPosition), Color.White);
            });

            if (_beingDragged)
            {
                // slot based on current mouse position if being dragged
                var currentSlot = GetCurrentSlotBasedOnPosition();
                var drawPosition = GetPosition(currentSlot) + _oldOffset;

                if (!_dragPositioned)
                    _dragPositioned = InventoryGridArea.Contains(DrawArea.WithPosition(drawPosition));

                if (_dragPositioned || InventoryGridArea.Contains(DrawArea.WithPosition(drawPosition)))
                    _spriteBatch.Draw(_itemGraphic, DrawPosition, Color.FromNonPremultiplied(255, 255, 255, 128));
            }
            else
            {
                _spriteBatch.Draw(_itemGraphic, DrawPositionWithParentOffset, Color.FromNonPremultiplied(255, 255, 255, 255));
            }

            _spriteBatch.End();
            base.OnDrawControl(gameTime);
        }

        private void InventoryPanelItem_OnMouseOver(object sender, MouseStateExtended e)
        {
            if (!InventoryGridArea.Contains(e.Position))
                return;

            var currentSlot = GetCurrentSlotBasedOnPosition();
            _highlightDrawPosition = Option.Some(GetPosition(currentSlot) + (_beingDragged ? _oldOffset : ImmediateParent.DrawPositionWithParentOffset));
        }

        protected override void HandleDragStart(IXNAControl control, MouseEventArgs eventArgs)
        {
            StartDragging();
        }

        protected override void HandleDrag(IXNAControl control, MouseEventArgs eventArgs)
        {
            DrawPosition = eventArgs.Position.ToVector2() - new Vector2(DrawArea.Width / 2, DrawArea.Height / 2);
        }

        protected override void HandleDragEnd(IXNAControl control, MouseEventArgs eventArgs)
        {
            StopDragging();
        }

        protected override void HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            if (_followMouse)
            {
                StopDragging();
            }
            else
            {
                StartDragging();
                _followMouse = true;
            }
        }

        protected override void HandleDoubleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            DoubleClick?.Invoke(control, Data);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _nameLabel.Dispose();
                _highlightBackground.Dispose();
            }

            base.Dispose(disposing);
        }

        private void UpdateNameLabelPosition()
        {
            if (_nameLabel == null)
                return;

            // the name label is parented to the inventory panel so that all name labels draw over all items (see draw orders below)
            // the actual position of the name label needs to be set to this control's draw position
            var actualPosition = DrawPosition;

            if (actualPosition.X + _nameLabel.DrawAreaWithParentOffset.Width + DrawArea.Width > InventoryGridArea.Width)
            {
                _nameLabel.DrawPosition = new Vector2(actualPosition.X - _nameLabel.DrawArea.Width, actualPosition.Y);
            }
            else
            {
                _nameLabel.DrawPosition = new Vector2(actualPosition.X + DrawArea.Width, actualPosition.Y);
            }

            DrawOrder = 110;
            _nameLabel.DrawOrder = 200;
        }

        private void StopDragging()
        {
            var args = new ItemDragCompletedEventArgs(Data);
            DoneDragging?.Invoke(this, args);

            if (!args.ContinueDrag)
            {
                if (!args.RestoreOriginalSlot)
                    Slot = GetCurrentSlotBasedOnPosition();
                else
                    DrawPosition = GetPosition(Slot);

                _dragPositioned = false;
                _beingDragged = false;
                _nameLabel.Visible = false;
                _oldOffset = Vector2.Zero;

                _followMouse = false;
            }
            else
            {
                _followMouse = true;
            }
        }

        private static Vector2 GetPosition(int slot)
        {
            return new Vector2(13 + 26 * (slot % InventoryPanel.InventoryRowSlots), 9 + 26 * (slot / InventoryPanel.InventoryRowSlots));
        }
    }
}
