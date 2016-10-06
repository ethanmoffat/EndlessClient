// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public class ArrowKeyHandler : InputHandlerBase
    {
        private readonly IArrowKeyController _arrowKeyController;

        public ArrowKeyHandler(IEndlessGameProvider endlessGameProvider,
                               IKeyStateProvider keyStateProvider,
                               IUserInputTimeRepository userInputTimeRepository,
                               IArrowKeyController arrowKeyController)
            : base(endlessGameProvider.Game as Game,
                keyStateProvider,
                userInputTimeRepository)
        {
            _arrowKeyController = arrowKeyController;
        }

        protected override Optional<Keys> HandleInput(GameTime gameTime)
        {
            if (IsKeyHeld(Keys.Left) && _arrowKeyController.MoveLeft())
                return Keys.Left;
            if (IsKeyHeld(Keys.Right) && _arrowKeyController.MoveRight())
                return Keys.Right;
            if (IsKeyHeld(Keys.Up) && _arrowKeyController.MoveUp())
                return Keys.Up;
            if (IsKeyHeld(Keys.Down) && _arrowKeyController.MoveDown())
                return Keys.Down;

            return Optional<Keys>.Empty;
        }

        private bool IsKeyHeld(Keys key)
        {
            return CurrentState.IsKeyHeld(PreviousState, key);
        }
    }
}
