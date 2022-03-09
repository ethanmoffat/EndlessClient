using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public class ArrowKeyHandler : InputHandlerBase
    {
        private readonly IArrowKeyController _arrowKeyController;

        public ArrowKeyHandler(IEndlessGameProvider endlessGameProvider,
                               IUserInputProvider userInputProvider,
                               IUserInputTimeRepository userInputTimeRepository,
                               IArrowKeyController arrowKeyController,
                               ICurrentMapStateProvider currentMapStateProvider)
            : base(endlessGameProvider, userInputProvider, userInputTimeRepository, currentMapStateProvider)
        {
            _arrowKeyController = arrowKeyController;
        }

        protected override Optional<Keys> HandleInput()
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
    }
}
