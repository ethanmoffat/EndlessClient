using AutomaticTypeMapper;
using EndlessClient.Controllers;
using EndlessClient.Dialogs;
using EndlessClient.GameExecution;
using EOLib.Domain.Map;

namespace EndlessClient.Input
{
    [MappedType(BaseType = typeof(IUserInputHandlerFactory))]
    public class UserInputHandlerFactory : IUserInputHandlerFactory
    {
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly IUserInputProvider _userInputProvider;
        private readonly IUserInputTimeRepository _userInputTimeRepository;
        private readonly IArrowKeyController _arrowKeyController;
        private readonly IControlKeyController _controlKeyController;
        private readonly IFunctionKeyController _functionKeyController;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IActiveDialogProvider _activeDialogProvider;

        public UserInputHandlerFactory(IEndlessGameProvider endlessGameProvider,
                                       IUserInputProvider userInputProvider,
                                       IUserInputTimeRepository userInputTimeRepository,
                                       IArrowKeyController arrowKeyController,
                                       IControlKeyController controlKeyController,
                                       IFunctionKeyController functionKeyController,
                                       ICurrentMapStateProvider  currentMapStateProvider,
                                       IActiveDialogProvider activeDialogProvider)
        {
            _endlessGameProvider = endlessGameProvider;
            _userInputProvider = userInputProvider;
            _userInputTimeRepository = userInputTimeRepository;
            _arrowKeyController = arrowKeyController;
            _controlKeyController = controlKeyController;
            _functionKeyController = functionKeyController;
            _currentMapStateProvider = currentMapStateProvider;
            _activeDialogProvider = activeDialogProvider;
        }

        public IUserInputHandler CreateUserInputHandler()
        {
            return new UserInputHandler(_endlessGameProvider,
                                        _userInputProvider,
                                        _userInputTimeRepository,
                                        _arrowKeyController,
                                        _controlKeyController,
                                        _functionKeyController,
                                        _currentMapStateProvider,
                                        _activeDialogProvider);
        }
    }

    public interface IUserInputHandlerFactory
    {
        IUserInputHandler CreateUserInputHandler();
    }
}
