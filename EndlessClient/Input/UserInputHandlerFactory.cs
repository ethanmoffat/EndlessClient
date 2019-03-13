// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using AutomaticTypeMapper;
using EndlessClient.Controllers;
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
        private readonly ICurrentMapStateProvider _currentMapStateProvider;

        public UserInputHandlerFactory(IEndlessGameProvider endlessGameProvider,
                                       IKeyStateProvider keyStateProvider,
                                       IUserInputTimeRepository userInputTimeRepository,
                                       IArrowKeyController arrowKeyController,
                                       IControlKeyController controlKeyController,
                                       ICurrentMapStateProvider  currentMapStateProvider)
        {
            _endlessGameProvider = endlessGameProvider;
            _keyStateProvider = keyStateProvider;
            _userInputTimeRepository = userInputTimeRepository;
            _arrowKeyController = arrowKeyController;
            _controlKeyController = controlKeyController;
            _currentMapStateProvider = currentMapStateProvider;
        }

        public IUserInputHandler CreateUserInputHandler()
        {
            return new UserInputHandler(_endlessGameProvider,
                                        _keyStateProvider,
                                        _userInputTimeRepository,
                                        _arrowKeyController,
                                        _controlKeyController,
                                        _currentMapStateProvider);
        }
    }

    public interface IUserInputHandlerFactory
    {
        IUserInputHandler CreateUserInputHandler();
    }
}
