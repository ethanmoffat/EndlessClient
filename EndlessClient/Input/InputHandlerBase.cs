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
        private static DateTime _lastInputTime = DateTime.Now; //todo: put this in a repo instead of making static

        private readonly IKeyStateProvider _keyStateProvider;
        protected KeyboardState CurrentState { get { return _keyStateProvider.CurrentKeyState; } }
        protected KeyboardState PreviousState { get { return _keyStateProvider.PreviousKeyState; } }

        protected InputHandlerBase(Game game, IKeyStateProvider keyStateProvider)
            : base(game)
        {
            _keyStateProvider = keyStateProvider;
        }

        public override void Update(GameTime gameTime)
        {
            var timeAtBeginningOfUpdate = DateTime.Now;
            var millisecondsSinceLastUpdate = (timeAtBeginningOfUpdate - _lastInputTime).TotalMilliseconds;
            if (!Game.IsActive ||
                millisecondsSinceLastUpdate < INPUT_RATE_LIMIT_MILLISECONDS ||
                XNAControl.Dialogs.Count > 0)
                return;

            var handledKey = HandleInput(gameTime);
            if (handledKey.HasValue)
                _lastInputTime = timeAtBeginningOfUpdate;

            base.Update(gameTime);
        }

        protected abstract Optional<Keys> HandleInput(GameTime gameTime);
    }
}