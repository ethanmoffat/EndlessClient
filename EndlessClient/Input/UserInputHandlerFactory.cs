using AutomaticTypeMapper;
using EndlessClient.Controllers;
using EndlessClient.Dialogs;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EndlessClient.Rendering;
using EOLib.Domain.Map;

namespace EndlessClient.Input;

[MappedType(BaseType = typeof(IUserInputHandlerFactory))]
public class UserInputHandlerFactory : IUserInputHandlerFactory
{
    private readonly IEndlessGameProvider _endlessGameProvider;
    private readonly IUserInputProvider _userInputProvider;
    private readonly IUserInputTimeRepository _userInputTimeRepository;
    private readonly IArrowKeyController _arrowKeyController;
    private readonly IControlKeyController _controlKeyController;
    private readonly IFunctionKeyController _functionKeyController;
    private readonly INumPadController _numPadController;
    private readonly IHudButtonController _hudButtonController;
    private readonly ICurrentMapStateRepository _currentMapStateRepository;
    private readonly IActiveDialogProvider _activeDialogProvider;
    private readonly IClientWindowSizeProvider _clientWindowSizeProvider;

    public UserInputHandlerFactory(IEndlessGameProvider endlessGameProvider,
                                   IUserInputProvider userInputProvider,
                                   IUserInputTimeRepository userInputTimeRepository,
                                   IArrowKeyController arrowKeyController,
                                   IControlKeyController controlKeyController,
                                   IFunctionKeyController functionKeyController,
                                   INumPadController numPadController,
                                   IHudButtonController hudButtonController,
                                   ICurrentMapStateRepository currentMapStateRepository,
                                   IActiveDialogProvider activeDialogProvider,
                                   IClientWindowSizeProvider clientWindowSizeProvider)
    {
        _endlessGameProvider = endlessGameProvider;
        _userInputProvider = userInputProvider;
        _userInputTimeRepository = userInputTimeRepository;
        _arrowKeyController = arrowKeyController;
        _controlKeyController = controlKeyController;
        _functionKeyController = functionKeyController;
        _numPadController = numPadController;
        _hudButtonController = hudButtonController;
        _currentMapStateRepository = currentMapStateRepository;
        _activeDialogProvider = activeDialogProvider;
        _clientWindowSizeProvider = clientWindowSizeProvider;
    }

    public IUserInputHandler CreateUserInputHandler()
    {
        return new UserInputHandler(_endlessGameProvider,
                                    _userInputProvider,
                                    _userInputTimeRepository,
                                    _arrowKeyController,
                                    _controlKeyController,
                                    _functionKeyController,
                                    _numPadController,
                                    _hudButtonController,
                                    _currentMapStateRepository,
                                    _activeDialogProvider,
                                    _clientWindowSizeProvider);
    }
}

public interface IUserInputHandlerFactory
{
    IUserInputHandler CreateUserInputHandler();
}