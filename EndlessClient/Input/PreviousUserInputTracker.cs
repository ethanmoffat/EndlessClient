using EndlessClient.GameExecution;
using EndlessClient.Rendering;
using Microsoft.Xna.Framework;

namespace EndlessClient.Input
{
    public class PreviousUserInputTracker : GameComponent
    {
        private readonly IUserInputRepository _userInputRepository;
        private readonly IFixedTimeStepRepository _fixedTimeStepRepository;

        public PreviousUserInputTracker(
            IEndlessGameProvider endlessGameProvider,
            IUserInputRepository userInputRepository,
            IFixedTimeStepRepository fixedTimeStepRepository)
            : base((Game)endlessGameProvider.Game)
        {
            _userInputRepository = userInputRepository;
            _fixedTimeStepRepository = fixedTimeStepRepository;
            UpdateOrder = int.MaxValue;
        }

        public override void Update(GameTime gameTime)
        {
            _userInputRepository.PreviousKeyState = _userInputRepository.CurrentKeyState;
            _userInputRepository.PreviousMouseState = _userInputRepository.CurrentMouseState;

            if (_fixedTimeStepRepository.IsUpdateFrame)
                _fixedTimeStepRepository.RestartTimer();

            base.Update(gameTime);
        }
    }
}
