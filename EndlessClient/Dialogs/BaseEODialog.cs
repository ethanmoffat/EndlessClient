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

        public bool ChildControlClickHandled { get; set; }

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

            if (_isInGame())
                DrawPosition = new Vector2(DrawPosition.X, (330 - DrawArea.Height)/2f);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            ChildControlClickHandled = false;

            base.OnUpdateControl(gameTime);
        }
    }
}
