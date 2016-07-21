// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.Controllers.Repositories;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.Input;
using EndlessClient.UIControls;
using EOLib.Config;
using EOLib.Graphics;
using EOLib.IO.Repositories;

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
        private readonly IMainButtonControllerProvider _mainButtonControllerProvider;
        private readonly ICreateAccountControllerProvider _createAccountControllerProvider;
        private readonly ILoginControllerProvider _loginControllerProvider;
        private readonly ICharacterManagementControllerProvider _characterManagementControllerProvider;

        public ControlSetFactory(INativeGraphicsManager nativeGraphicsManager,
                                 IEOMessageBoxFactory messageBoxFactory,
                                 IHudControlsFactory hudControlsFactory,
                                 IContentManagerProvider contentManagerProvider,
                                 IKeyboardDispatcherProvider keyboardDispatcherProvider,
                                 IConfigurationProvider configProvider,
                                 ICharacterInfoPanelFactory characterInfoPanelFactory,
                                 IMainButtonControllerProvider mainButtonControllerProvider,
                                 ICreateAccountControllerProvider createAccountControllerProvider,
                                 ILoginControllerProvider loginControllerProvider,
                                 ICharacterManagementControllerProvider characterManagementControllerProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _messageBoxFactory = messageBoxFactory;
            _hudControlsFactory = hudControlsFactory;
            _contentManagerProvider = contentManagerProvider;
            _keyboardDispatcherProvider = keyboardDispatcherProvider;
            _configProvider = configProvider;
            _characterInfoPanelFactory = characterInfoPanelFactory;
            _mainButtonControllerProvider = mainButtonControllerProvider;
            _createAccountControllerProvider = createAccountControllerProvider;
            _loginControllerProvider = loginControllerProvider;
            _characterManagementControllerProvider = characterManagementControllerProvider;
        }

        public IControlSet CreateControlsForState(GameStates newState, IControlSet currentControlSet)
        {
            var controlSet = GetSetBasedOnState(newState);
            controlSet.InitializeResources(_nativeGraphicsManager, _contentManagerProvider.Content);
            controlSet.InitializeControls(currentControlSet);
            return controlSet;
        }

        private IControlSet GetSetBasedOnState(GameStates newState)
        {
            switch (newState)
            {
                case GameStates.Initial: return new InitialControlSet(_configProvider, MainButtonController);
                case GameStates.CreateAccount:
                    return new CreateAccountControlSet(
                        _keyboardDispatcherProvider.Dispatcher,
                        MainButtonController,
                        AccountController);
                case GameStates.Login:
                    return new LoginPromptControlSet(
                        _keyboardDispatcherProvider.Dispatcher,
                        _configProvider, 
                        MainButtonController,
                        LoginController);
                case GameStates.ViewCredits: return new ViewCreditsControlSet(_configProvider, MainButtonController);
                case GameStates.LoggedIn:
                    return new LoggedInControlSet(
                        _keyboardDispatcherProvider.Dispatcher,
                        MainButtonController,
                        _characterInfoPanelFactory,
                        CharacterManagementController,
                        AccountController);
                case GameStates.PlayingTheGame:
                    return new InGameControlSet(MainButtonController, _messageBoxFactory, _hudControlsFactory);
                default: throw new ArgumentOutOfRangeException("newState", newState, null);
            }
        }

        private IMainButtonController MainButtonController
        {
            get { return _mainButtonControllerProvider.MainButtonController; }
        }

        private IAccountController AccountController
        {
            get { return _createAccountControllerProvider.AccountController; }
        }

        private ILoginController LoginController
        {
            get { return _loginControllerProvider.LoginController; }
        }

        private ICharacterManagementController CharacterManagementController
        {
            get { return _characterManagementControllerProvider.CharacterManagementController; }
        }
    }
}
