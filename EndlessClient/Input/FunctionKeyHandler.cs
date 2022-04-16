using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Input;
using Optional;

namespace EndlessClient.Input
{
    public class FunctionKeyHandler : InputHandlerBase
    {
        private readonly IFunctionKeyController _functionKeyController;

        public FunctionKeyHandler(IEndlessGameProvider endlessGameProvider,
                                  IUserInputProvider userInputProvider,
                                  IUserInputTimeRepository userInputTimeRepository,
                                  IFunctionKeyController functionKeyController,
                                  ICurrentMapStateProvider currentMapStateProvider)
            : base(endlessGameProvider, userInputProvider, userInputTimeRepository, currentMapStateProvider)
        {
            _functionKeyController = functionKeyController;
        }

        protected override Option<Keys> HandleInput()
        {
            for (var key = Keys.F1; key < Keys.F8; key++)
            {
                if (IsKeyHeld(key))
                {
                    var isShiftHeld = IsKeyHeld(Keys.LeftShift) || IsKeyHeld(Keys.RightShift);
                    if (_functionKeyController.SelectSpell(key - Keys.F1, isShiftHeld))
                        return Option.Some(key);
                }
            }

            if (IsKeyPressedOnce(Keys.F11) && _functionKeyController.Sit())
                return Option.Some(Keys.F11);

            if (IsKeyPressedOnce(Keys.F12) && _functionKeyController.RefreshMapState())
                return Option.Some(Keys.F12);

            return Option.None<Keys>();
        }
    }
}
