// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.Controllers.Repositories;
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
        private readonly IMainButtonControllerRepository _mainButtonControllerRepository;
        private readonly ICreateAccountControllerRepository _createAccountControllerRepository;
        private readonly ILoginControllerRepository _loginControllerRepository;
        private readonly ICharacterManagementControllerRepository _characterManagementControllerRepository;

        public ControlSetFactory(INativeGraphicsManager nativeGraphicsManager,
                                 IEOMessageBoxFactory messageBoxFactory,
                                 IHudControlsFactory hudControlsFactory,
                                 IContentManagerProvider contentManagerProvider,
                                 IKeyboardDispatcherProvider keyboardDispatcherProvider,
                                 IConfigurationProvider configProvider,
                                 ICharacterInfoPanelFactory characterInfoPanelFactory,
                                 IMainButtonControllerRepository mainButtonControllerRepository,
                                 ICreateAccountControllerRepository createAccountControllerRepository,
                                 ILoginControllerRepository loginControllerRepository,
                                 ICharacterManagementControllerRepository characterManagementControllerRepository)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _messageBoxFactory = messageBoxFactory;
            _hudControlsFactory = hudControlsFactory;
            _contentManagerProvider = contentManagerProvider;
            _keyboardDispatcherProvider = keyboardDispatcherProvider;
            _configProvider = configProvider;
            _characterInfoPanelFactory = characterInfoPanelFactory;
            _mainButtonControllerRepository = mainButtonControllerRepository;
            _createAccountControllerRepository = createAccountControllerRepository;
            _loginControllerRepository = loginControllerRepository;
            _characterManagementControllerRepository = characterManagementControllerRepository;
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
            get { return _mainButtonControllerRepository.MainButtonController; }
        }

        private IAccountController AccountController
        {
            get { return _createAccountControllerRepository.AccountController; }
        }

        private ILoginController LoginController
        {
            get { return _loginControllerRepository.LoginController; }
        }

        private ICharacterManagementController CharacterManagementController
        {
            get { return _characterManagementControllerRepository.CharacterManagementController; }
        }
    }
}
