using EndlessClient.Audio;
using EndlessClient.Dialogs.Extensions;
using EndlessClient.HUD.Panels;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input;
using MonoGame.Extended.Input.InputListeners;
using Optional;
using System;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class PaperdollDialogItem : XNAPictureBox
    {
        private readonly ISfxPlayer _sfxPlayer;
        private readonly InventoryPanel _inventoryPanel;
        private readonly PaperdollDialog _paperdollDialog;
        private readonly bool _isMainCharacter;
        private readonly Option<EIFRecord> _itemInfo;

        private bool _beingDragged;

        public EquipLocation EquipLocation { get; }

        public int ItemID => _itemInfo.Match(r => r.ID, () => 0);

        public event Action<EIFRecord> UnequipAction;

        public bool IsBeingDragged => _beingDragged;

        public PaperdollDialogItem(INativeGraphicsManager nativeGraphicsManager,
                                   ISfxPlayer sfxPlayer,
                                   InventoryPanel inventoryPanel,
                                   PaperdollDialog paperdollDialog,
                                   bool isMainCharacter,
                                   EquipLocation location,
                                   Option<EIFRecord> itemInfo)
        {
            _sfxPlayer = sfxPlayer;
            _inventoryPanel = inventoryPanel;
            _paperdollDialog = paperdollDialog;
            _isMainCharacter = isMainCharacter;
            EquipLocation = location;
            _itemInfo = itemInfo;

            _itemInfo.MatchSome(r => Texture = nativeGraphicsManager.TextureFromResource(GFXTypes.Items, r.Graphic * 2, true));
            StretchMode = StretchMode.CenterInFrame;
        }

        protected override bool HandleClick(IXNAControl control, MouseEventArgs eventArgs)
        {
            if (!_isMainCharacter || !_itemInfo.HasValue)
                return false;

            if (eventArgs.Button == MouseButton.Left)
            {
                if (_inventoryPanel.MouseOver && _inventoryPanel.MouseOverPreviously)
                {
                    StopDragging();
                    _itemInfo.MatchSome(itemInfo => UnequipAction?.Invoke(itemInfo));
                }
            }
            else if (eventArgs.Button == MouseButton.Right)
            {
                if (_beingDragged)
                    StopDragging();
                else
                    _itemInfo.MatchSome(itemInfo => UnequipAction?.Invoke(itemInfo));
            }

            return true;
        }

        protected override bool HandleDragStart(IXNAControl control, MouseEventArgs eventArgs)
        {
            if (!_isMainCharacter || !_itemInfo.HasValue)
                return false;

            _beingDragged = true;
            SetControlUnparented();
            Game.Components.Add(this);

            _sfxPlayer.PlaySfx(SoundEffectID.InventoryPickup);

            DrawOrder = 1000;
            return true;
        }

        protected override bool HandleDrag(IXNAControl control, MouseEventArgs eventArgs)
        {
            if (!_isMainCharacter || !_itemInfo.HasValue)
                return false;

            DrawPosition = new Vector2(eventArgs.Position.X - (DrawArea.Width / 2), eventArgs.Position.Y - (DrawArea.Height / 2));

            return true;
        }

        protected override bool HandleDragEnd(IXNAControl control, MouseEventArgs eventArgs)
        {
            if (!_isMainCharacter || !_itemInfo.HasValue)
                return false;

            if (_inventoryPanel.MouseOver && _inventoryPanel.MouseOverPreviously)
                _itemInfo.MatchSome(itemInfo => UnequipAction?.Invoke(itemInfo));

            StopDragging();

            return true;
        }

        private void StopDragging()
        {
            _beingDragged = false;
            SetParentControl(_paperdollDialog);
            DrawArea = EquipLocation.GetEquipLocationRectangle();

            _sfxPlayer.PlaySfx(SoundEffectID.InventoryPlace);
        }
    }
}
