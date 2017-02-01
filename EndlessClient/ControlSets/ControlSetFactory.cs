// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Controls;
using EndlessClient.Input;
using EndlessClient.UIControls;
using EOLib.Config;
using EOLib.Graphics;

namespace EndlessClient.ControlSets
{
    public class ControlSetFactory : IControlSetFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IHudControlsFactory _hudControlsFactory;
        private readonly IContentManagerProvider _contentManagerProvider;
        private readonly IKeyboardDispatcherProvider _keyboardDispatcherProvider;
        private readonly IConfigurationProvider _configProvider;
        private readonly ICharacterInfoPanelFactory _characterInfoPanelFactory;
        private IMainButtonController _mainButtonController;
        private IAccountController _accountController;
        private ILoginController _loginController;
        private ICharacterManagementController _characterManagementController;

        public ControlSetFactory(INativeGraphicsManager nativeGraphicsManager,
                                 IEOMessageBoxFactory messageBoxFactory,
                                 IHudControlsFactory hudControlsFactory,
                                 IContentManagerProvider contentManagerProvider,
                                 IKeyboardDispatcherProvider keyboardDispatcherProvider,
                                 IConfigurationProvider configProvider,
                                 ICharacterInfoPanelFactory characterInfoPanelFactory)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _messageBoxFactory = messageBoxFactory;
            _hudControlsFactory = hudControlsFactory;
            _contentManagerProvider = contentManagerProvider;
            _keyboardDispatcherProvider = keyboardDispatcherProvider;
            _configProvider = configProvider;
            _characterInfoPanelFactory = characterInfoPanelFactory;
        }

        public IControlSet CreateControlsForState(GameStates newState, IControlSet currentControlSet)
        {
            if (_mainButtonController == null || _accountController == null ||
                _loginController == null || _characterManagementController == null)
                throw new InvalidOperationException("Missing controllers - the Unity container was initialized incorrectly");

            var controlSet = GetSetBasedOnState(newState);
            controlSet.InitializeResources(_nativeGraphicsManager, _contentManagerProvider.Content);
            controlSet.InitializeControls(currentControlSet);
            return controlSet;
        }

        public void InjectControllers(IMainButtonController mainButtonController,
                                      IAccountController accountController,
                                      ILoginController loginController,
                                      ICharacterManagementController characterManagementController)
        {
            _mainButtonController = mainButtonController;
            _accountController = accountController;
            _loginController = loginController;
            _characterManagementController = characterManagementController;
        }

        private IControlSet GetSetBasedOnState(GameStates newState)
        {
            switch (newState)
            {
                case GameStates.Initial: return new InitialControlSet(_configProvider, _mainButtonController);
                case GameStates.CreateAccount:
                    return new CreateAccountControlSet(
                        _keyboardDispatcherProvider.Dispatcher,
                        _mainButtonController,
                        _accountController);
                case GameStates.Login:
                    return new LoginPromptControlSet(
                        _keyboardDispatcherProvider.Dispatcher,
                        _configProvider,
                        _mainButtonController,
                        _loginController);
                case GameStates.ViewCredits: return new ViewCreditsControlSet(_configProvider, _mainButtonController);
                case GameStates.LoggedIn:
                    return new LoggedInControlSet(
                        _keyboardDispatcherProvider.Dispatcher,
                        _mainButtonController,
                        _characterInfoPanelFactory,
                        _characterManagementController,
                        _accountController);
                case GameStates.PlayingTheGame:
                    return new InGameControlSet(_mainButtonController, _messageBoxFactory, _hudControlsFactory);
                default: throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
            }
        }
    }
}
