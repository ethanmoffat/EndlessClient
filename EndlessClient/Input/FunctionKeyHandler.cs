using EndlessClient.Controllers;
using EndlessClient.GameExecution;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Input;
using Optional;

namespace EndlessClient.Input
{
    public class FunctionKeyHandler : InputHandlerBase
    {
        private readonly IFunctionKeyController _functionKeyController;
        private readonly IPlayerInfoProvider _playerInfoProvider;

        public FunctionKeyHandler(IEndlessGameProvider endlessGameProvider,
                                  IUserInputProvider userInputProvider,
                                  IUserInputTimeRepository userInputTimeRepository,
                                  IFunctionKeyController functionKeyController,
                                  ICurrentMapStateRepository currentMapStateRepository,
                                  IPlayerInfoProvider playerInfoProvider)
            : base(endlessGameProvider, userInputProvider, userInputTimeRepository, currentMapStateRepository, playerInfoProvider)
        {
            _functionKeyController = functionKeyController;
            _playerInfoProvider = playerInfoProvider;
        }

        protected override Option<Keys> HandleInput()
        {
            for (var key = Keys.F1; key <= Keys.F8; key++)
            {
                if (IsKeyHeld(key))
                {
                    var isShiftHeld = IsKeyHeld(Keys.LeftShift) || IsKeyHeld(Keys.RightShift);
                    var isFrozen = _playerInfoProvider.IsPlayerFrozen;

                    if (isFrozen)
                        return Option.None<Keys>();

					if (_functionKeyController.SelectSpell(key - Keys.F1, isShiftHeld) )
                        return Option.Some(key);
                }
            }

			if (IsKeyPressedOnce(Keys.F11) && !_playerInfoProvider.IsPlayerFrozen && _functionKeyController.Sit())
                return Option.Some(Keys.F11);

            if (IsKeyPressedOnce(Keys.F12) && _functionKeyController.RefreshMapState())
                return Option.Some(Keys.F12);

            return Option.None<Keys>();
        }
    }
}
