// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Actions;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EndlessClient.HUD.Chat;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using EndlessClient.Input;
using EndlessClient.Network;
using EndlessClient.Rendering;
using EndlessClient.Subscribers;
using EndlessClient.Test;
using EndlessClient.UIControls;
using EOLib.DependencyInjection;
using EOLib.Domain.Notifiers;
using Microsoft.Practices.Unity;
using XNAControls;

namespace EndlessClient
{
    public class EndlessClientDependencyContainer : IInitializableContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterInstance<IEndlessGame, EndlessGame>()
                .RegisterType<ITestModeLauncher, TestModeLauncher>();

            //factories
            container.RegisterInstance<IControlSetFactory, ControlSetFactory>()
                .RegisterInstance<IHudControlsFactory, HudControlsFactory>()
                .RegisterInstance<ICharacterInfoPanelFactory, CharacterInfoPanelFactory>()
                .RegisterType<IHudPanelFactory, HudPanelFactory>()
                .RegisterType<IUserInputHandlerFactory, UserInputHandlerFactory>();

            container.RegisterType<IEOMessageBoxFactory, EOMessageBoxFactory>()
                .RegisterType<ICreateAccountWarningDialogFactory, CreateAccountWarningDialogFactory>()
                .RegisterType<ICreateAccountProgressDialogFactory, CreateAccountProgressDialogFactory>()
                .RegisterType<ICreateCharacterDialogFactory, CreateCharacterDialogFactory>()
                .RegisterType<IChangePasswordDialogFactory, ChangePasswordDialogFactory>()
                .RegisterType<IGameLoadingDialogFactory, GameLoadingDialogFactory>();

            container.RegisterType<IEODialogButtonService, EODialogButtonService>();

            //notifiers
            container
                .RegisterVaried<IChatEventNotifier, ChatNotificationActions>();

            //provider/repository
            container.RegisterInstance<IGameStateProvider, GameStateRepository>()
                .RegisterInstance<IGameStateRepository, GameStateRepository>()
                .RegisterInstance<IContentManagerProvider, ContentManagerRepository>()
                .RegisterInstance<IContentManagerRepository, ContentManagerRepository>()
                .RegisterInstance<IKeyboardDispatcherProvider, KeyboardDispatcherRepository>()
                .RegisterInstance<IKeyboardDispatcherRepository, KeyboardDispatcherRepository>()
                .RegisterInstance<IControlSetProvider, ControlSetRepository>()
                .RegisterInstance<IControlSetRepository, ControlSetRepository>()
                .RegisterInstance<IEndlessGameProvider, EndlessGameRepository>()
                .RegisterInstance<IEndlessGameRepository, EndlessGameRepository>()
                .RegisterInstance<IStatusLabelTextProvider, StatusLabelTextRepository>()
                .RegisterInstance<IStatusLabelTextRepository, StatusLabelTextRepository>()
                .RegisterInstance<IKeyStateRepository, KeyStateRepository>()
                .RegisterInstance<IKeyStateProvider, KeyStateRepository>();

            container.RegisterInstance<IUserInputTimeRepository, UserInputTimeRepository>();

            //provider only
            container.RegisterInstance<IClientWindowSizeProvider, ClientWindowSizeProvider>()
                .RegisterInstance<IHudControlProvider, HudControlProvider>();

            //controllers
            container.RegisterType<IMainButtonController, MainButtonController>()
                .RegisterType<IAccountController, AccountController>()
                .RegisterType<ILoginController, LoginController>()
                .RegisterType<ICharacterManagementController, CharacterManagementController>()
                .RegisterType<IChatController, ChatController>()
                .RegisterType<IArrowKeyController, ArrowKeyController>()
                .RegisterType<IControlKeyController, ControlKeyController>();

            //actions
            container.RegisterType<IGameStateActions, GameStateActions>()
                .RegisterType<IErrorDialogDisplayAction, ErrorDialogDisplayAction>()
                .RegisterType<IAccountDialogDisplayActions, AccountDialogDisplayActions>()
                .RegisterType<ICharacterDialogActions, CharacterDialogActions>()
                .RegisterType<IChatTextBoxActions, ChatTextBoxActions>()
                .RegisterType<IFirstTimePlayerActions, FirstTimePlayerActions>()
                .RegisterType<IPrivateMessageActions, PrivateMessageActions>()
                .RegisterType<IChatBubbleActions, ChatBubbleActions>()
                .RegisterType<IWalkErrorHandler, WalkErrorHandler>();

            //hud
            container.RegisterType<IHudButtonController, HudButtonController>()
                .RegisterType<IHudStateActions, HudStateActions>()
                .RegisterType<IStatusLabelSetter, StatusLabelSetter>()
                .RegisterType<IChatModeCalculator, ChatModeCalculator>();

            RegisterSubscribers(container);
        }

        public void InitializeDependencies(IUnityContainer container)
        {
            SetUpRepositoriesForGameDependencies(container);
            MethodInjectControllers(container);
        }

        private void RegisterSubscribers(IUnityContainer container)
        {
            container.RegisterVaried<IMainCharacterEventNotifier, MainCharacterEventSubscriber>()
                .RegisterVaried<IOtherCharacterEventNotifier, OtherCharacterEventSubscriber>();
        }

        private static void SetUpRepositoriesForGameDependencies(IUnityContainer container)
        {
            var game = container.Resolve<IEndlessGame>();
            var gameRepository = container.Resolve<IEndlessGameRepository>();
            gameRepository.Game = game;
            gameRepository.Game.Components.Add(container.Resolve<PacketHandlerGameComponent>());

            var contentRepo = container.Resolve<IContentManagerRepository>();
            contentRepo.Content = game.Content;

            var keyboardDispatcherRepo = container.Resolve<IKeyboardDispatcherRepository>();
            keyboardDispatcherRepo.Dispatcher = new KeyboardDispatcher(game.Window);
        }

        private static void MethodInjectControllers(IUnityContainer container)
        {
            var mainButtonController = container.Resolve<IMainButtonController>();
            var accountController = container.Resolve<IAccountController>();
            var loginController = container.Resolve<ILoginController>();
            var characterManagementController = container.Resolve<ICharacterManagementController>();
            var chatController = container.Resolve<IChatController>();

            var controlSetFactory = container.Resolve<IControlSetFactory>();
            controlSetFactory.InjectControllers(mainButtonController,
                                                accountController,
                                                loginController,
                                                characterManagementController);

            var charInfoPanelFactory = container.Resolve<ICharacterInfoPanelFactory>();
            charInfoPanelFactory.InjectLoginController(loginController);
            charInfoPanelFactory.InjectCharacterManagementController(characterManagementController);

            var hudControlsFactory = container.Resolve<IHudControlsFactory>();
            hudControlsFactory.InjectChatController(chatController);
        }
    }
}
