using EndlessClient.GameExecution;
using Microsoft.Xna.Framework;
using System;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public abstract class BaseEODialog : XNADialog
    {
        private readonly Func<bool> _isInGame;

        protected BaseEODialog(IGameStateProvider gameStateProvider)
        {
            _isInGame = () => gameStateProvider.CurrentState == GameStates.PlayingTheGame;
        }

        protected BaseEODialog(bool isInGame)
        {
            _isInGame = () => isInGame;
        }

        public override void CenterInGameView()
        {
            base.CenterInGameView();

            if (_isInGame())
                DrawPosition = new Vector2(DrawPosition.X, (330 - DrawArea.Height)/2f);
        }
    }
}
