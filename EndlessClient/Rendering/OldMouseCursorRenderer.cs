using System;
using System.Linq;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Old;
using EndlessClient.Old;
using EOLib.IO.Map;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Rendering
{
    public sealed class OldMouseCursorRenderer
    {
        private readonly EOGame _game;
        private readonly OldMapRenderer _parentMapRenderer;
        
        //private readonly EOMapContextMenu _contextMenu;
        private readonly OldCharacter _mainCharacter;

        private int _gridX, _gridY;
        private Rectangle _cursorSourceRect;

        private IMapFile MapRef => _parentMapRenderer.MapRef;

        public Point GridCoords => new Point(_gridX, _gridY);

        public OldMouseCursorRenderer(EOGame game, OldMapRenderer parentMapRenderer)
        {
            _game = game;
            _parentMapRenderer = parentMapRenderer;

            //_contextMenu = new EOMapContextMenu(_game.API);

            _mainCharacter = OldWorld.Instance.MainPlayer.ActiveCharacter;
        }

        public void ShowContextMenu(OldCharacterRenderer player)
        {
            //_contextMenu.SetCharacterRenderer(player);
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

        private void HandleTileSpecClick(TileSpec spec)
        {
            switch (spec)
            {
                case TileSpec.BankVault: HandleBankVaultClick(); break;
                //todo: boards, chairs
            }
        }

        private void HandleBankVaultClick()
        {
            var characterWithinOneUnitOfLocker = Math.Max(_mainCharacter.X - _gridX, _mainCharacter.Y - _gridY) <= 1;
            var characterInSameRowOrColAsLocker = _gridX == _mainCharacter.X || _gridY == _mainCharacter.Y;

            if (characterWithinOneUnitOfLocker && characterInSameRowOrColAsLocker)
                LockerDialog.Show(_game.API, (byte) _gridX, (byte) _gridY);
        }
    }
}
