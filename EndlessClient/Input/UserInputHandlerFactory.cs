// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Controllers;
using EndlessClient.GameExecution;

namespace EndlessClient.Input
{
    public class UserInputHandlerFactory : IUserInputHandlerFactory
    {
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly IKeyStateProvider _keyStateProvider;
        private readonly IUserInputTimeRepository _userInputTimeRepository;
        private readonly IArrowKeyController _arrowKeyController;
        private readonly IControlKeyController _controlKeyController;

        public UserInputHandlerFactory(IEndlessGameProvider endlessGameProvider,
                                       IKeyStateProvider keyStateProvider,
                                       IUserInputTimeRepository userInputTimeRepository,
                                       IArrowKeyController arrowKeyController,
                                       IControlKeyController controlKeyController)
        {
            _endlessGameProvider = endlessGameProvider;
            _keyStateProvider = keyStateProvider;
            _userInputTimeRepository = userInputTimeRepository;
            _arrowKeyController = arrowKeyController;
            _controlKeyController = controlKeyController;
        }

        public IUserInputHandler CreateUserInputHandler()
        {
            return new UserInputHandler(_endlessGameProvider,
                                        _keyStateProvider,
                                        _userInputTimeRepository,
                                        _arrowKeyController,
                                        _controlKeyController);
        }
    }

    public interface IUserInputHandlerFactory
    {
        IUserInputHandler CreateUserInputHandler();
    }
}
