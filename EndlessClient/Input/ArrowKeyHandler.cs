// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public class ArrowKeyHandler : GameComponent
    {
        private readonly IKeyStateProvider _keyStateProvider;
        private readonly IArrowKeyController _arrowKeyController;

        public ArrowKeyHandler(IEndlessGameProvider endlessGameProvider,
                               IKeyStateProvider keyStateProvider,
                               IArrowKeyController arrowKeyController)
            : base(endlessGameProvider.Game as Game)
        {
            _keyStateProvider = keyStateProvider;
            _arrowKeyController = arrowKeyController;
        }

        public override void Update(GameTime gameTime)
        {
            //todo: rate limit input; ignore input when dialogs are shown

            if (IsKeyHeld(Keys.Left))
                _arrowKeyController.MoveLeft();
            else if (IsKeyHeld(Keys.Right))
                _arrowKeyController.MoveRight();
            else if (IsKeyHeld(Keys.Up))
                _arrowKeyController.MoveUp();
            else if (IsKeyHeld(Keys.Down))
                _arrowKeyController.MoveDown();

            base.Update(gameTime);
        }

        private bool IsKeyHeld(Keys key)
        {
            return _keyStateProvider.CurrentKeyState.IsKeyHeld(_keyStateProvider.CurrentKeyState, key);
        }
    }
}
