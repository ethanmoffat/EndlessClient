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
using XNAControls;

namespace EndlessClient.HUD.Inventory
{
    public class InventoryPanelItem : XNAControl
    {
        private readonly IItemStringService _itemStringService;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly InventoryPanel _inventoryPanel;
        private readonly EIFRecord _data;

        private readonly Texture2D _itemGraphic;
        private readonly Texture2D _highlightBackground;
        private readonly XNALabel _nameLabel;

        private int _slot;
        private IInventoryItem _item;

        public int Slot
        {
            get => _slot;
            set
            {
                _slot = value;
                DrawPosition = GetPosition(_slot);
            }
        }

        public IInventoryItem InventoryItem
        {
            get => _item;
            set
            {
                _item = value;
                _nameLabel.Text = _itemStringService.GetStringForMapDisplay(_data, _item.Amount);
                _nameLabel.ResizeBasedOnText(16, 9);
            }
        }

        public InventoryPanelItem(
            INativeGraphicsManager nativeGraphicsManager,
            IItemStringService itemStringService,
            IStatusLabelSetter statusLabelSetter,
            InventoryPanel inventoryPanel, int slot, IInventoryItem inventoryItem, EIFRecord data)
        {
            _itemStringService = itemStringService;
            _statusLabelSetter = statusLabelSetter;
            _inventoryPanel = inventoryPanel;
            _data = data;
            Slot = slot;
            _item = inventoryItem;

            _itemGraphic = nativeGraphicsManager.TextureFromResource(GFXTypes.Items, 2 * data.Graphic, transparent: true);
            _highlightBackground = new Texture2D(Game.GraphicsDevice, 1, 1);
            _highlightBackground.SetData(new[] { Color.FromNonPremultiplied(200, 200, 200, 60) });

            _nameLabel = new XNALabel(Constants.FontSize08)
            {
                Visible = false,
                AutoSize = false,
                TextAlign = LabelAlignment.MiddleCenter,
                ForeColor = ColorConstants.LightGrayText,
                BackColor = Color.FromNonPremultiplied(30, 30, 30, 160),
                Text = _itemStringService.GetStringForMapDisplay(data, inventoryItem.Amount)
            };

            OnMouseEnter += (_, _) =>
            {
                _nameLabel.Visible = true;
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ITEM, _nameLabel.Text);
            };
            OnMouseLeave += (_, _) => _nameLabel.Visible = false;

            var (slotWidth, slotHeight) = _data.Size.GetDimensions();
            SetSize(slotWidth * 26, slotHeight * 26);
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
