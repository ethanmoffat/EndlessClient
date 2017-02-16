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

        private readonly Texture2D _mouseCursor;
        private readonly XNALabel _itemHoverName;
        private readonly EOMapContextMenu _contextMenu;
        private readonly OldCharacter _mainCharacter;

        private Vector2 _cursorPos;
        private int _gridX, _gridY;
        private Rectangle _cursorSourceRect;
        private MouseState _prevState;
        private bool _hideCursor;

        private IMapFile MapRef => _parentMapRenderer.MapRef;

        public Point GridCoords => new Point(_gridX, _gridY);

        public OldMouseCursorRenderer(EOGame game, OldMapRenderer parentMapRenderer)
        {
            _game = game;
            _parentMapRenderer = parentMapRenderer;

            _mouseCursor = game.GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 24, true);
            _itemHoverName = new XNALabel(new Rectangle(1, 1, 1, 1), Constants.FontSize08pt75)
            {
                Visible = true,
                Text = "",
                ForeColor = Color.White,
                DrawOrder = (int)ControlDrawLayer.BaseLayer + 3,
                AutoSize = false
            };

            _cursorSourceRect = new Rectangle(0, 0, _mouseCursor.Width / 5, _mouseCursor.Height);

            _contextMenu = new EOMapContextMenu(_game.API);

            _mainCharacter = OldWorld.Instance.MainPlayer.ActiveCharacter;
        }

        public void ShowContextMenu(OldCharacterRenderer player)
        {
            _contextMenu.SetCharacterRenderer(player);
        }

        public void Update()
        {
            var ms = Mouse.GetState();

            UpdateCursorInfo(ms);
            UpdateDisplayedMapItemName();
            HandleMouseClick(ms);

            _prevState = ms;
        }

        public void Draw(SpriteBatch sb, bool beginHasBeenCalled = true)
        {
            if (_hideCursor)
                return;

            if (_gridX >= 0 && _gridY >= 0 && _gridX <= MapRef.Properties.Width && _gridY <= MapRef.Properties.Height)
            {
                //don't draw cursor if context menu is visible and the context menu has the mouse over it
                if (!(_contextMenu.Visible && _contextMenu.MouseOver))
                {
                    if (!beginHasBeenCalled)
                        sb.Begin();

                    sb.Draw(_mouseCursor, _cursorPos, _cursorSourceRect, Color.White);

                    if (!beginHasBeenCalled)
                        sb.End();
                }
            }
        }

        private void UpdateCursorInfo(MouseState ms)
        {
            //don't do the cursor if there is a dialog open or the mouse is over the context menu
            if (XNAControl.Dialogs.Count > 0 || (_contextMenu.Visible && _contextMenu.MouseOver))
                return;

            SetGridCoordsBasedOnMousePosition(ms);
            _cursorPos = OldMapRenderer.GetDrawCoordinatesFromGridUnits(_gridX, _gridY, _mainCharacter);

            var ti = GetTileInfoAtGridCoordinates();
            if (ti == null) return;

            _hideCursor = false;
            //switch (ti.ReturnType)
            //{
            //    case TileInfoReturnType.IsOtherPlayer:
            //    case TileInfoReturnType.IsOtherNPC:
            //        _cursorSourceRect.Location = new Point(_mouseCursor.Width / 5, 0);
            //        break;
            //    case TileInfoReturnType.IsTileSpec:
            //        UpdateCursorForTileSpec(ti.Spec);
            //        break;
            //    case TileInfoReturnType.IsMapSign:
            //        _hideCursor = true;
            //        break;
            //    case TileInfoReturnType.IsWarpSpec:
            //        _cursorSourceRect.Location = new Point(0, 0);
            //        break;
            //}
        }

        private void UpdateDisplayedMapItemName()
        {
            var mi = _parentMapRenderer.GetMapItemAt(_gridX, _gridY);

            if (mi != null)
            {
                _cursorSourceRect.Location = new Point(2 * (_mouseCursor.Width / 5), 0);

                string itemName = OldEOInventoryItem.GetNameString(mi.ItemID, mi.Amount);
                if (_itemHoverName.Text != itemName)
                {
                    _itemHoverName.Visible = true;
                    _itemHoverName.Text = OldEOInventoryItem.GetNameString(mi.ItemID, mi.Amount);
                    _itemHoverName.ResizeBasedOnText();
                    _itemHoverName.ForeColor = OldEOInventoryItem.GetItemTextColor(mi.ItemID);
                }
                _itemHoverName.DrawLocation = new Vector2(
                    _cursorPos.X + 32 - _itemHoverName.ActualWidth / 2f,
                    _cursorPos.Y - _itemHoverName.ActualHeight - 4);
            }
            else if (_itemHoverName.Visible)
            {
                _itemHoverName.Visible = false;
                _itemHoverName.Text = " ";
            }

            if (_itemHoverName.Text.Length > 0 && !_game.Components.Contains(_itemHoverName))
                _game.Components.Add(_itemHoverName);
        }

        private void UpdateCursorForTileSpec(TileSpec spec)
        {
            switch (spec)
            {
                case TileSpec.Wall:
                case TileSpec.JammedDoor:
                case TileSpec.MapEdge:
                case TileSpec.FakeWall:
                    _hideCursor = true;
                    break;
                case TileSpec.Chest:
                case TileSpec.BankVault:
                case TileSpec.ChairDown:
                case TileSpec.ChairLeft:
                case TileSpec.ChairRight:
                case TileSpec.ChairUp:
                case TileSpec.ChairDownRight:
                case TileSpec.ChairUpLeft:
                case TileSpec.ChairAll:
                case TileSpec.Board1:
                case TileSpec.Board2:
                case TileSpec.Board3:
                case TileSpec.Board4:
                case TileSpec.Board5:
                case TileSpec.Board6:
                case TileSpec.Board7:
                case TileSpec.Board8:
                case TileSpec.Jukebox:
                    _cursorSourceRect.Location = new Point(_mouseCursor.Width / 5, 0);
                    break;
                case TileSpec.Jump:
                case TileSpec.Water:
                case TileSpec.Arena:
                case TileSpec.AmbientSource:
                case TileSpec.SpikesStatic:
                case TileSpec.SpikesTrap:
                case TileSpec.SpikesTimed:
                case TileSpec.None:
                    _cursorSourceRect.Location = new Point(0, 0);
                    break;
            }
        }

        private void SetGridCoordsBasedOnMousePosition(MouseState ms)
        {
            //need to solve this system of equations to get x, y on the grid
            //(x * 32) - (y * 32) + 288 - c.OffsetX, => pixX = 32x - 32y + 288 - c.OffsetX
            //(y * 16) + (x * 16) + 144 - c.OffsetY  => 2pixY = 32y + 32x + 288 - 2c.OffsetY
            //                                         => 2pixY + pixX = 64x + 576 - c.OffsetX - 2c.OffsetY
            //                                         => 2pixY + pixX - 576 + c.OffsetX + 2c.OffsetY = 64x
            //                                         => _gridX = (pixX + 2pixY - 576 + c.OffsetX + 2c.OffsetY) / 64; <=
            //pixY = (_gridX * 16) + (_gridY * 16) + 144 - c.OffsetY =>
            //(pixY - (_gridX * 16) - 144 + c.OffsetY) / 16 = _gridY

            //center the cursor on the mouse pointer
            var msX = ms.X - _cursorSourceRect.Width/2;
            var msY = ms.Y - _cursorSourceRect.Height/2;
            //align cursor to grid based on mouse position
            _gridX = (int) Math.Round((msX + 2*msY - 576 + _mainCharacter.OffsetX + 2*_mainCharacter.OffsetY)/64.0);
            _gridY = (int) Math.Round((msY - _gridX*16 - 144 + _mainCharacter.OffsetY)/16.0);
        }

        private IMapCellState GetTileInfoAtGridCoordinates()
        {
            if (_gridX >= 0 && _gridX <= MapRef.Properties.Width && _gridY >= 0 && _gridY <= MapRef.Properties.Height)
                return _parentMapRenderer.GetTileInfo((byte)_gridX, (byte)_gridY);

            return null;
        }

        private void HandleMouseClick(MouseState ms)
        {
            bool mouseClicked = ms.LeftButton == ButtonState.Released && _prevState.LeftButton == ButtonState.Pressed;
            //bool rightClicked = ms.RightButton == ButtonState.Released && _prevState.RightButton == ButtonState.Pressed;

            //don't handle mouse clicks for map if there is a dialog being shown
            mouseClicked &= XNAControl.Dialogs.Count == 0;
            //rightClicked &= XNAControl.Dialogs.Count == 0;

            var ti = GetTileInfoAtGridCoordinates();
            if (mouseClicked && ti != null)
            {
                var topMapItem = _parentMapRenderer.GetMapItemAt(_gridX, _gridY);
                if (topMapItem != null)
                    HandleMapItemClick(topMapItem);

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
            }
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
                _itemHoverName.Close();
                _contextMenu.Close();
            }
        }

        #endregion
    }
}
