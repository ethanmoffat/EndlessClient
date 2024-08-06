using EndlessClient.GameExecution;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public class CurrentUserInputTracker : GameComponent
    {
        private readonly IUserInputRepository _userInputRepository;

        public CurrentUserInputTracker(
            IEndlessGameProvider endlessGameProvider,
            IUserInputRepository userInputRepository)
            : base((Game)endlessGameProvider.Game)
        {
            _userInputRepository = userInputRepository;

            UpdateOrder = int.MinValue;
        }

        public override void Update(GameTime gameTime)
        {
            _userInputRepository.CurrentKeyState = Keyboard.GetState();
            _userInputRepository.CurrentMouseState = Mouse.GetState();

            base.Update(gameTime);
        }
    }
}
