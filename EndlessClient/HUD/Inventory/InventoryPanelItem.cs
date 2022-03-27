using EndlessClient.HUD.Panels;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Item;
using EOLib.Graphics;
using EOLib.IO.Extensions;
using EOLib.IO.Pub;
using EOLib.Localization;
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
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly InventoryPanel _inventoryPanel;
        private readonly EIFRecord _data;

        private readonly Texture2D _itemGraphic;
        private readonly Texture2D _highlightBackground;
        private readonly XNALabel _nameLabel;

        private int _slot;

        private readonly Stopwatch _clickTimer;
        private int _recentClicks;

        public int Slot
        {
            get => _slot;
            set
            {
                _slot = value;
                DrawPosition = GetPosition(_slot);
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

        public event EventHandler<EIFRecord> DoubleClick;

        public InventoryPanelItem(InventoryPanel inventoryPanel, int slot, IInventoryItem inventoryItem, EIFRecord data)
        {
            _inventoryPanel = inventoryPanel;
            Slot = slot;
            InventoryItem = inventoryItem;
            _data = data;

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

            OnMouseEnter += (_, _) => _nameLabel.Visible = true;
            OnMouseLeave += (_, _) => _nameLabel.Visible = false;

            var (slotWidth, slotHeight) = _data.Size.GetDimensions();
            SetSize(slotWidth * 26, slotHeight * 26);

            _clickTimer = new Stopwatch();
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
            if (_recentClicks > 0 && _clickTimer.Elapsed.TotalSeconds > 1)
            {
                _clickTimer.Restart();
                _recentClicks--;
            }

            if (MouseOver && MouseOverPreviously)
            {
                if (CurrentMouseState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed)
                {
                    _clickTimer.Restart();
                    _recentClicks++;

                    if (_recentClicks == 2)
                    {
                        DoubleClick?.Invoke(this, _data);
                        _recentClicks = 0;
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
                _spriteBatch.Draw(_highlightBackground, DrawAreaWithParentOffset.WithSize(DrawArea.Width - 3, DrawArea.Height - 3), Color.White);
            }

            _spriteBatch.Draw(_itemGraphic, DrawPositionWithParentOffset, Color.White);

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
