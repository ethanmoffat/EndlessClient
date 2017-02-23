// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using EndlessClient.Dialogs;
using EndlessClient.HUD.Inventory;
using EndlessClient.Old;
using EOLib;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls.Old;

namespace EndlessClient.Rendering
{
    public sealed class OldMouseCursorRenderer : IDisposable
    {
        private readonly EOGame _game;
        private readonly OldMapRenderer _parentMapRenderer;
        
        private readonly EOMapContextMenu _contextMenu;
        private readonly OldCharacter _mainCharacter;

        private int _gridX, _gridY;
        private Rectangle _cursorSourceRect;

        private IMapFile MapRef => _parentMapRenderer.MapRef;

        public Point GridCoords => new Point(_gridX, _gridY);

        public OldMouseCursorRenderer(EOGame game, OldMapRenderer parentMapRenderer)
        {
            _game = game;
            _parentMapRenderer = parentMapRenderer;

            _contextMenu = new EOMapContextMenu(_game.API);

            _mainCharacter = OldWorld.Instance.MainPlayer.ActiveCharacter;
        }

        public void ShowContextMenu(OldCharacterRenderer player)
        {
            _contextMenu.SetCharacterRenderer(player);
        }

        public void Update()
        {
            HandleMouseClick(Mouse.GetState());
        }

        public void Draw(SpriteBatch sb, bool beginHasBeenCalled = true) { }

        private void HandleMouseClick(MouseState ms)
        {
            //bool mouseClicked = ms.LeftButton == ButtonState.Released && _prevState.LeftButton == ButtonState.Pressed;
            //bool rightClicked = ms.RightButton == ButtonState.Released && _prevState.RightButton == ButtonState.Pressed;

            //don't handle mouse clicks for map if there is a dialog being shown
            //mouseClicked &= XNAControl.Dialogs.Count == 0;
            //rightClicked &= XNAControl.Dialogs.Count == 0;

            //var ti = GetTileInfoAtGridCoordinates();
            //if (mouseClicked && ti != null)
            //{
            //    var topMapItem = _parentMapRenderer.GetMapItemAt(_gridX, _gridY);
            //    if (topMapItem != null)
            //        HandleMapItemClick(topMapItem);

                //switch (ti.ReturnType)
                //{
                //    case TileInfoReturnType.IsMapSign:
                //        var signInfo = (MapSign)ti.MapElement;
                //        EOMessageBox.Show(signInfo.Message, signInfo.Title, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                //        break;
                //    case TileInfoReturnType.IsOtherPlayer:
                //        break;
                //    case TileInfoReturnType.IsOtherNPC:
                //        break;
                //    case TileInfoReturnType.IsTileSpec:
                //        HandleTileSpecClick(ti.Spec);
                //        break;
                //    default:
                //        if (_mainCharacter.NeedsSpellTarget)
                //        {
                //            //cancel spell targeting if an invalid target was selected
                //            _mainCharacter.SelectSpell(-1);
                //        }
                //        break;
                //}
            //}
        }

        private void HandleMapItemClick(OldMapItem mi)
        {
            if ((_mainCharacter.ID != mi.OwningPlayerID && mi.OwningPlayerID != 0) &&
                (mi.IsNPCDrop && (DateTime.Now - mi.DropTime).TotalSeconds <= OldWorld.Instance.NPCDropProtectTime) ||
                (!mi.IsNPCDrop && (DateTime.Now - mi.DropTime).TotalSeconds <= OldWorld.Instance.PlayerDropProtectTime))
            {
                OldCharacter charRef = _parentMapRenderer.GetOtherPlayerByID((short) mi.OwningPlayerID);
                EOResourceID msg = charRef == null ? EOResourceID.STATUS_LABEL_ITEM_PICKUP_PROTECTED : EOResourceID.STATUS_LABEL_ITEM_PICKUP_PROTECTED_BY;
                string extra = charRef == null ? "" : charRef.Name;
                EOGame.Instance.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, msg, extra);
            }
            else
            {
                var item = OldWorld.Instance.EIF[mi.ItemID];
                if (!EOGame.Instance.Hud.InventoryFits(mi.ItemID))
                {
                    EOGame.Instance.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT);
                }
                else if (_mainCharacter.Weight + item.Weight * mi.Amount > _mainCharacter.MaxWeight)
                {
                    EOGame.Instance.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.DIALOG_ITS_TOO_HEAVY_WEIGHT);
                }
                else if (!_game.API.GetItem(mi.UniqueID)) //server validates drop protection anyway
                    EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
            }
        }

        private void HandleTileSpecClick(TileSpec spec)
        {
            switch (spec)
            {
                case TileSpec.Chest: HandleChestClick(); break;
                case TileSpec.BankVault: HandleBankVaultClick(); break;
                //todo: boards, chairs
            }
        }

        private void HandleChestClick()
        {
            var characterWithinOneUnitOfChest = Math.Max(_mainCharacter.X - _gridX, _mainCharacter.Y - _gridY) <= 1;
            var characterInSameRowOrColAsChest = _gridX == _mainCharacter.X || _gridY == _mainCharacter.Y;

            if (characterWithinOneUnitOfChest && characterInSameRowOrColAsChest)
            {
                var chest = MapRef.Chests.Single(_mc => _mc.X == _gridX && _mc.Y == _gridY);
                if (chest == null) return;

                string requiredKey;
                switch (_mainCharacter.CanOpenChest(chest))
                {
                    case ChestKey.Normal: requiredKey = "Normal Key"; break;
                    case ChestKey.Silver: requiredKey = "Silver Key"; break;
                    case ChestKey.Crystal: requiredKey = "Crystal Key"; break;
                    case ChestKey.Wraith: requiredKey = "Wraith Key"; break;
                    default: ChestDialog.Show(_game.API, (byte)chest.X, (byte)chest.Y); return;
                }
                
                EOMessageBox.Show(DialogResourceID.CHEST_LOCKED, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                _game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING,
                    EOResourceID.STATUS_LABEL_THE_CHEST_IS_LOCKED_EXCLAMATION,
                    " - " + requiredKey);
            }
        }

        private void HandleBankVaultClick()
        {
            var characterWithinOneUnitOfLocker = Math.Max(_mainCharacter.X - _gridX, _mainCharacter.Y - _gridY) <= 1;
            var characterInSameRowOrColAsLocker = _gridX == _mainCharacter.X || _gridY == _mainCharacter.Y;

            if (characterWithinOneUnitOfLocker && characterInSameRowOrColAsLocker)
                LockerDialog.Show(_game.API, (byte) _gridX, (byte) _gridY);
        }

        #region IDisposable

        ~OldMouseCursorRenderer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _contextMenu.Close();
            }
        }

        #endregion
    }
}
