using EndlessClient.GameExecution;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public abstract class BaseEODialog : XNADialog
    {
        private readonly IGameStateProvider _gameStateProvider;

        protected BaseEODialog(IGameStateProvider gameStateProvider)
        {
            _gameStateProvider = gameStateProvider;
        }

        public override void CenterInGameView()
        {
            base.CenterInGameView();

            if (_gameStateProvider.CurrentState == GameStates.PlayingTheGame)
                DrawPosition = new Vector2(DrawPosition.X, (330 - DrawArea.Height)/2f);
        }
    }
}
