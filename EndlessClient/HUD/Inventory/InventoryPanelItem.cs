using EndlessClient.HUD.Panels;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO.Extensions;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
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
        private static readonly Rectangle InventoryGridArea = new Rectangle(110, 334, 377, 116);

        private readonly InventoryPanel _inventoryPanel;
        private readonly Texture2D _itemGraphic;
        private readonly Texture2D _highlightBackground;
        private readonly XNALabel _nameLabel;

        private int _slot;

        private readonly Stopwatch _clickTimer;
        private int _recentClicks;

        private ulong _updateTick;

        // Ru Paul's drag properties
        private bool _beingDragged;
        private Vector2 _oldOffset;

        private bool MousePressed => CurrentMouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Released;
        
        private bool MouseReleased => CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed;

        private bool MouseHeld => CurrentMouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Pressed;

        public int Slot
        {
            get => _slot;
            set
            {
                _slot = value;
                DrawPosition = GetPosition(_slot);
                DrawOrder = 102 - (_slot % InventoryPanel.InventoryRowSlots) * 2;
            }
        }

        public IInventoryItem InventoryItem { get; set; }

        public string Text
        {
            get => _nameLabel.Text;
            set
            {
                _nameLabel.Text = value;
                _nameLabel.ResizeBasedOnText(16, 9);
            }
        }

        public bool IsDragging => _beingDragged;

        public EIFRecord Data { get; }

        public event EventHandler<EIFRecord> DoubleClick;
        public event EventHandler<ItemDragCompletedEventArgs> DoneDragging;

        public InventoryPanelItem(InventoryPanel inventoryPanel, int slot, IInventoryItem inventoryItem, EIFRecord data)
        {
            _inventoryPanel = inventoryPanel;
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
                ForeColor = ColorConstants.LightGrayText,
                BackColor = Color.FromNonPremultiplied(30, 30, 30, 160),
                Text = string.Empty
            };

            OnMouseEnter += (_, _) => _nameLabel.Visible = !_beingDragged;
            OnMouseLeave += (_, _) => _nameLabel.Visible = false;

            var (slotWidth, slotHeight) = Data.Size.GetDimensions();
            SetSize(slotWidth * 26 - 3, slotHeight * 26 - 3);

            _clickTimer = new Stopwatch();
        }

        public int GetCurrentSlotBasedOnPosition()
        {
            if (!_beingDragged)
                return Slot;

            return (int)((DrawPosition.X - _oldOffset.X) / 26) + InventoryPanel.InventoryRowSlots * (int)((DrawPosition.Y - _oldOffset.Y) / 26);
        }

        public void StartDragging()
        {
            _beingDragged = true;
            _nameLabel.Visible = false;

            _oldOffset = DrawPositionWithParentOffset - DrawPosition;

            // todo: drag without unparenting this control
            SetControlUnparented();
            AddControlToDefaultGame();

            DrawOrder = 1000;
        }

        public override void Initialize()
        {
            _nameLabel.Initialize();
            _nameLabel.SetParentControl(this);
            _nameLabel.ResizeBasedOnText(16, 9);

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_recentClicks > 0)
            {
                if (_clickTimer.Elapsed.TotalMilliseconds > 500)
                {
                    _clickTimer.Restart();
                    _recentClicks--;
                }
                else if (_clickTimer.Elapsed.TotalMilliseconds > 200 && _inventoryPanel.NoItemsDragging())
                {
                    StartDragging();
                }
            }

            if (MousePressed)
                _updateTick = 0;

            if (!_beingDragged && MouseOver && MouseOverPreviously && MouseReleased)
            {
                _clickTimer.Restart();
                _recentClicks++;

                if (_recentClicks == 2)
                {
                    DoubleClick?.Invoke(this, Data);
                    _recentClicks = 0;
                }
            }
            else if (++_updateTick % 8 == 0 && !_beingDragged && MouseOver && MouseOverPreviously && MouseHeld)
            {
                if (_inventoryPanel.NoItemsDragging())
                {
                    StartDragging();
                }
            }
            else if (_beingDragged)
            {
                DrawPosition = new Vector2(CurrentMouseState.X - (DrawArea.Width / 2), CurrentMouseState.Y - (DrawArea.Height / 2));

                if (MouseReleased)
                {
                    var args = new ItemDragCompletedEventArgs(Data);
                    DoneDragging?.Invoke(this, args);

                    if (!args.ContinueDrag)
                    {
                        if (Game.Components.Contains(this))
                            Game.Components.Remove(this);

                        SetParentControl(_inventoryPanel);

                        if (!args.RestoreOriginalSlot)
                            Slot = GetCurrentSlotBasedOnPosition();
                        else
                            DrawPosition = GetPosition(Slot);

                        _beingDragged = false;
                        _nameLabel.Visible = false;
                    }
                }
            }

            base.OnUpdateControl(gameTime);
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            _spriteBatch.Begin();

            if (MouseOver)
            {
                // slot based on current mouse position if being dragged
                var currentSlot = GetCurrentSlotBasedOnPosition();
                var drawPosition = GetPosition(currentSlot) + (_beingDragged ? _oldOffset : ImmediateParent.DrawPositionWithParentOffset);

                if (InventoryGridArea.Contains(drawPosition))
                {
                    _spriteBatch.Draw(_highlightBackground, DrawArea.WithPosition(drawPosition), Color.White);
                }
            }

            _spriteBatch.Draw(_itemGraphic, DrawPositionWithParentOffset, Color.FromNonPremultiplied(255, 255, 255, _beingDragged ? 128 : 255));

            _spriteBatch.End();

            base.OnDrawControl(gameTime);
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

        private static Vector2 GetPosition(int slot)
        {
            return new Vector2(13 + 26 * (slot % InventoryPanel.InventoryRowSlots), 9 + 26 * (slot / InventoryPanel.InventoryRowSlots));
        }
    }
}
