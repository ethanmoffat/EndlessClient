// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.Input
{
    public abstract class InputHandlerBase : GameComponent
    {
        private const int INPUT_RATE_LIMIT_MILLISECONDS = 200;

        private readonly IKeyStateProvider _keyStateProvider;
        private readonly IUserInputTimeRepository _userInputTimeRepository;

        private KeyboardState CurrentState { get { return _keyStateProvider.CurrentKeyState; } }

        private KeyboardState PreviousState { get { return _keyStateProvider.PreviousKeyState; } }

        protected InputHandlerBase(Game game,
            IKeyStateProvider keyStateProvider,
            IUserInputTimeRepository userInputTimeRepository)
            : base(game)
        {
            _keyStateProvider = keyStateProvider;
            _userInputTimeRepository = userInputTimeRepository;
        }

        public override void Update(GameTime gameTime)
        {
            var timeAtBeginningOfUpdate = DateTime.Now;
            var millisecondsSinceLastUpdate = GetMillisecondsSinceLastUpdate(timeAtBeginningOfUpdate);
            if (!Game.IsActive ||
                millisecondsSinceLastUpdate < INPUT_RATE_LIMIT_MILLISECONDS ||
                XNAControl.Dialogs.Count > 0)
                return;

            var handledKey = HandleInput(gameTime);
            if (handledKey.HasValue)
                _userInputTimeRepository.LastInputTime = timeAtBeginningOfUpdate;

            base.Update(gameTime);
        }

        private double GetMillisecondsSinceLastUpdate(DateTime timeAtBeginningOfUpdate)
        {
            return (timeAtBeginningOfUpdate - _userInputTimeRepository.LastInputTime).TotalMilliseconds;
        }

        protected abstract Optional<Keys> HandleInput(GameTime gameTime);

        protected bool IsKeyHeld(Keys key)
        {
            return CurrentState.IsKeyHeld(PreviousState, key);
        }
    }
}