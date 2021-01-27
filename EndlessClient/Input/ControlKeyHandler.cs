using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public class ControlKeyHandler : InputHandlerBase
    {
        private readonly IControlKeyController _controlKeyController;

        public ControlKeyHandler(IEndlessGameProvider endlessGameProvider,
                                 IUserInputProvider userInputProvider,
                                 IUserInputTimeRepository userInputTimeRepository,
                                 IControlKeyController controlKeyController,
                                 ICurrentMapStateProvider currentMapStateProvider)
            : base(endlessGameProvider, userInputProvider, userInputTimeRepository, currentMapStateProvider)
        {
            _controlKeyController = controlKeyController;
        }

        protected override Optional<Keys> HandleInput()
        {
            if (IsKeyHeld(Keys.LeftControl) && _controlKeyController.Attack())
                return Keys.LeftControl;
            if (IsKeyHeld(Keys.RightControl) && _controlKeyController.Attack())
                return Keys.RightControl;

            return Optional<Keys>.Empty;
        }
    }
}
