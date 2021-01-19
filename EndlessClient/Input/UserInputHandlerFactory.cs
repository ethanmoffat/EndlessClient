using AutomaticTypeMapper;
using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.GameExecution;
using EOLib.Domain.Map;

namespace EndlessClient.Input
{
    [MappedType(BaseType = typeof(IUserInputHandlerFactory))]
    public class UserInputHandlerFactory : IUserInputHandlerFactory
    {
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly IKeyStateProvider _keyStateProvider;
        private readonly IUserInputTimeRepository _userInputTimeRepository;
        private readonly IArrowKeyController _arrowKeyController;
        private readonly IControlKeyController _controlKeyController;
        private readonly IFunctionKeyController _functionKeyController;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IHudControlProvider _hudControlProvider;

        public UserInputHandlerFactory(IEndlessGameProvider endlessGameProvider,
                                       IKeyStateProvider keyStateProvider,
                                       IUserInputTimeRepository userInputTimeRepository,
                                       IArrowKeyController arrowKeyController,
                                       IControlKeyController controlKeyController,
                                       IFunctionKeyController functionKeyController,
                                       ICurrentMapStateProvider  currentMapStateProvider,
                                       IHudControlProvider hudControlProvider)
        {
            _endlessGameProvider = endlessGameProvider;
            _keyStateProvider = keyStateProvider;
            _userInputTimeRepository = userInputTimeRepository;
            _arrowKeyController = arrowKeyController;
            _controlKeyController = controlKeyController;
            _functionKeyController = functionKeyController;
            _currentMapStateProvider = currentMapStateProvider;
            _hudControlProvider = hudControlProvider;
        }

        public IUserInputHandler CreateUserInputHandler()
        {
            return new UserInputHandler(_endlessGameProvider,
                                        _keyStateProvider,
                                        _userInputTimeRepository,
                                        _arrowKeyController,
                                        _controlKeyController,
                                        _functionKeyController,
                                        _currentMapStateProvider,
                                        _hudControlProvider);
        }
    }

    public interface IUserInputHandlerFactory
    {
        IUserInputHandler CreateUserInputHandler();
    }
}
