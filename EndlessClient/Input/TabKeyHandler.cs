using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Input;
using Optional;

namespace EndlessClient.Input
{
    public class TabKeyHandler : InputHandlerBase
    {
        private readonly ITabKeyController _tabKeyController;

        public TabKeyHandler(IEndlessGameProvider endlessGameProvider,
                                 IUserInputProvider userInputProvider,
                                 IUserInputTimeRepository userInputTimeRepository,
                                 ITabKeyController tabKeyController,
                                 ICurrentMapStateRepository currentMapStateRepository)
            : base(endlessGameProvider, userInputProvider, userInputTimeRepository, currentMapStateRepository)
        {
            _tabKeyController = tabKeyController;
        }

        protected override Option<Keys> HandleInput()
        {
            if (IsKeyPressedOnce(Keys.Tab)) {
                _tabKeyController.ToggleMapView();
                return Option.Some(Keys.Tab);
            }

            return Option.None<Keys>();
        }
    }
}
