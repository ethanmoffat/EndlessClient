using System;
using System.Linq;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Input
{
    public abstract class InputHandlerBase : IInputHandler
    {
        private const int INPUT_RATE_LIMIT_MILLISECONDS = 200;

        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly IUserInputProvider _keyStateProvider;
        private readonly IUserInputTimeRepository _userInputTimeRepository;
        private readonly ICurrentMapStateProvider _mapStateProvider;

        private KeyboardState CurrentState => _keyStateProvider.CurrentKeyState;

        private KeyboardState PreviousState => _keyStateProvider.PreviousKeyState;

        protected InputHandlerBase(IEndlessGameProvider endlessGameProvider,
                                   IUserInputProvider userInputProvider,
                                   IUserInputTimeRepository userInputTimeRepository,
                                   ICurrentMapStateProvider mapStateProvider)
        {
            _endlessGameProvider = endlessGameProvider;
            _keyStateProvider = userInputProvider;
            _userInputTimeRepository = userInputTimeRepository;
            _mapStateProvider = mapStateProvider;
        }

        public void HandleKeyboardInput(DateTime timeAtBeginningOfUpdate)
        {
            var millisecondsSinceLastUpdate = GetMillisecondsSinceLastUpdate(timeAtBeginningOfUpdate);
            if (!_endlessGameProvider.Game.IsActive ||
                millisecondsSinceLastUpdate < INPUT_RATE_LIMIT_MILLISECONDS ||
                _mapStateProvider.MapWarpState != WarpState.None)
                return;

            var handledKey = HandleInput();
            if (handledKey.HasValue)
                _userInputTimeRepository.LastInputTime = timeAtBeginningOfUpdate;
        }

        private double GetMillisecondsSinceLastUpdate(DateTime timeAtBeginningOfUpdate)
        {
            return (timeAtBeginningOfUpdate - _userInputTimeRepository.LastInputTime).TotalMilliseconds;
        }

        protected abstract Optional<Keys> HandleInput();

        protected bool IsKeyHeld(params Keys[] keys)
        {
            return keys.Any(key => CurrentState.IsKeyHeld(PreviousState, key));
        }

        protected bool IsKeyPressedOnce(params Keys[] keys)
        {
            return keys.Any(key => CurrentState.IsKeyPressedOnce(PreviousState, key));
        }
    }

    public interface IInputHandler
    {
        void HandleKeyboardInput(DateTime timeAtStart);
    }
}