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
    public class ControlKeyHandler : InputHandlerBase
    {
        private readonly IControlKeyController _controlKeyController;

        public ControlKeyHandler(IEndlessGameProvider endlessGameProvider,
                                 IKeyStateProvider keyStateProvider,
                                 IUserInputTimeRepository userInputTimeRepository,
                                 IControlKeyController controlKeyController)
            : base((Game)endlessGameProvider.Game, keyStateProvider, userInputTimeRepository)
        {
            _controlKeyController = controlKeyController;
        }

        protected override Optional<Keys> HandleInput(GameTime gameTime)
        {
            if (IsKeyHeld(Keys.LeftControl) && _controlKeyController.Attack())
                return Keys.LeftControl;
            if (IsKeyHeld(Keys.RightControl) && _controlKeyController.Attack())
                return Keys.RightControl;

            return Optional<Keys>.Empty;
        }
    }
}
