// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.GameExecution;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public class InGameKeyStateTrackerComponent : GameComponent
    {
        private readonly IKeyStateRepository _keyStateRepository;

        public InGameKeyStateTrackerComponent(
            IEndlessGameProvider endlessGameProvider,
            IKeyStateRepository keyStateRepository)
            : base((Game)endlessGameProvider.Game)
        {
            _keyStateRepository = keyStateRepository;

            UpdateOrder = int.MaxValue;
        }

        public override void Update(GameTime gameTime)
        {
            _keyStateRepository.PreviousKeyState = Keyboard.GetState();

            base.Update(gameTime);
        }
    }
}
