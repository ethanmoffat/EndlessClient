using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Controls;
using EOLib;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public class ArrowKeyHandler : InputHandlerBase
    {
        private readonly IArrowKeyController _arrowKeyController;
        private readonly IHudControlProvider _hudControlProvider;

        public ArrowKeyHandler(IEndlessGameProvider endlessGameProvider,
                               IKeyStateProvider keyStateProvider,
                               IUserInputTimeRepository userInputTimeRepository,
                               IArrowKeyController arrowKeyController,
                               ICurrentMapStateProvider currentMapStateProvider,
                               IHudControlProvider hudControlProvider)
            : base(endlessGameProvider, keyStateProvider, userInputTimeRepository, currentMapStateProvider)
        {
            _arrowKeyController = arrowKeyController;
            _hudControlProvider = hudControlProvider;
        }

        protected override Optional<Keys> HandleInput()
        {
            if (IsKeyHeld(Keys.Left, Keys.Right, Keys.Up, Keys.Down))
            {
                _hudControlProvider.GetComponent<IClickWalkPathHandler>(
                    HudControlIdentifier.ClickWalkPathHandler).CancelWalking();
            }

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
