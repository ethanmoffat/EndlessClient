using EndlessClient.GameExecution;
using Microsoft.Xna.Framework;

namespace EndlessClient.Input
{
    public class PreviousUserInputTracker : GameComponent
    {
        private readonly IUserInputRepository _userInputRepository;

        public PreviousUserInputTracker(
            IEndlessGameProvider endlessGameProvider,
            IUserInputRepository userInputRepository)
            : base((Game)endlessGameProvider.Game)
        {
            _userInputRepository = userInputRepository;

            UpdateOrder = int.MaxValue;
        }

        public override void Update(GameTime gameTime)
        {
            _userInputRepository.PreviousKeyState = _userInputRepository.CurrentKeyState;
            _userInputRepository.PreviousMouseState = _userInputRepository.CurrentMouseState;
            _userInputRepository.ClickHandled = false;
            _userInputRepository.WalkClickHandled = false;

            base.Update(gameTime);
        }
    }
}
