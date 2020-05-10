using EndlessClient.GameExecution;
using Microsoft.Xna.Framework;

namespace EndlessClient.Input
{
    public class PreviousKeyStateTracker : GameComponent
    {
        private readonly IKeyStateRepository _keyStateRepository;

        public PreviousKeyStateTracker(
            IEndlessGameProvider endlessGameProvider,
            IKeyStateRepository keyStateRepository)
            : base((Game)endlessGameProvider.Game)
        {
            _keyStateRepository = keyStateRepository;

            UpdateOrder = int.MaxValue;
        }

        public override void Update(GameTime gameTime)
        {
            _keyStateRepository.PreviousKeyState = _keyStateRepository.CurrentKeyState;

            base.Update(gameTime);
        }
    }
}
