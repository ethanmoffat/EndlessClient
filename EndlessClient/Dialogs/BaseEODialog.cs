using EndlessClient.GameExecution;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using System;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public abstract class BaseEODialog : XNADialog
    {
        private readonly Func<bool> _isInGame;

        public INativeGraphicsManager GraphicsManager { get; }

        protected BaseEODialog(INativeGraphicsManager graphicsManager,
                               IGameStateProvider gameStateProvider)
        {
            GraphicsManager = graphicsManager;
            _isInGame = () => gameStateProvider.CurrentState == GameStates.PlayingTheGame;
        }

        protected BaseEODialog(INativeGraphicsManager graphicsManager, bool isInGame)
        {
            GraphicsManager = graphicsManager;
            _isInGame = () => isInGame;
        }

        public override void CenterInGameView()
        {
            base.CenterInGameView();

            if (_isInGame() && !Game.Window.AllowUserResizing)
                DrawPosition = new Vector2(DrawPosition.X, (330 - DrawArea.Height)/2f);
        }

        public void Close()
        {
            Close(XNADialogResult.NO_BUTTON_PRESSED);
        }
    }
}
