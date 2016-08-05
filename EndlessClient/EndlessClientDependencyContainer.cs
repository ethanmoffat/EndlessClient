// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Content;
using EndlessClient.Controllers;
using EndlessClient.Controllers.Repositories;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Actions;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using EndlessClient.Input;
using EndlessClient.Network;
using EndlessClient.Rendering;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.Map;
using EndlessClient.Rendering.MapEntityRenderers;
using EndlessClient.Rendering.Sprites;
using EndlessClient.UIControls;
using EOLib.DependencyInjection;
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
            container.RegisterType<IControlSetFactory, ControlSetFactory>()
                .RegisterType<IHudControlsFactory, HudControlsFactory>()
                .RegisterType<ICharacterRendererFactory, CharacterRendererFactory>()
                .RegisterType<ICharacterInfoPanelFactory, CharacterInfoPanelFactory>()
                .RegisterType<IHudPanelFactory, HudPanelFactory>()
                .RegisterType<IRenderTargetFactory, RenderTargetFactory>()
                .RegisterType<ICharacterPropertyRendererBuilder, CharacterPropertyRendererBuilder>()
                .RegisterType<IMapRendererFactory, MapRendererFactory>();

            container.RegisterType<IEOMessageBoxFactory, EOMessageBoxFactory>()
                .RegisterType<ICreateAccountWarningDialogFactory, CreateAccountWarningDialogFactory>()
                .RegisterType<ICreateAccountProgressDialogFactory, CreateAccountProgressDialogFactory>()
                .RegisterType<ICreateCharacterDialogFactory, CreateCharacterDialogFactory>()
                .RegisterType<IChangePasswordDialogFactory, ChangePasswordDialogFactory>()
                .RegisterType<IGameLoadingDialogFactory, GameLoadingDialogFactory>();

            //services
            container.RegisterType<IMapRenderDistanceCalculator, MapRenderDistanceCalculator>()
                .RegisterType<ICharacterRenderOffsetCalculator, CharacterRenderOffsetCalculator>()
                .RegisterType<ICharacterTextures, CharacterTextures>()
                .RegisterType<ICharacterSpriteCalculator, CharacterSpriteCalculator>();

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
                .RegisterInstance<ICharacterRendererProvider, CharacterRendererRepository>()
                .RegisterInstance<ICharacterRendererRepository, CharacterRendererRepository>();

            //provider only
            container.RegisterInstance<IClientWindowSizeProvider, ClientWindowSizeProvider>()
                .RegisterInstance<IHudControlProvider, HudControlProvider>()
                .RegisterInstance<IMapEntityRendererProvider, MapEntityRendererProvider>()
                .RegisterInstance<IMapItemGraphicProvider, MapItemGraphicProvider>()
                .RegisterInstance<ICharacterStateCache, CharacterStateCache>();

            //controllers
            container.RegisterType<IMainButtonController, MainButtonController>()
                .RegisterType<IAccountController, AccountController>()
                .RegisterType<ILoginController, LoginController>()
                .RegisterType<ICharacterManagementController, CharacterManagementController>();
            
            //controller provider/repository (bad hack - avoids circular dependency)
            container.RegisterInstance<IMainButtonControllerProvider, MainButtonControllerRepository>()
                .RegisterInstance<IMainButtonControllerRepository, MainButtonControllerRepository>()
                .RegisterInstance<ICreateAccountControllerProvider, CreateAccountControllerRepository>()
                .RegisterInstance<ICreateAccountControllerRepository, CreateAccountControllerRepository>()
                .RegisterInstance<ILoginControllerProvider, LoginControllerRepository>()
                .RegisterInstance<ILoginControllerRepository, LoginControllerRepository>()
                .RegisterInstance<ICharacterManagementControllerProvider, CharacterManagementRepository>()
                .RegisterInstance<ICharacterManagementControllerRepository, CharacterManagementRepository>();
            
            //actions
            container.RegisterType<IGameStateActions, GameStateActions>()
                .RegisterType<IErrorDialogDisplayAction, ErrorDialogDisplayAction>()
                .RegisterType<IAccountDialogDisplayActions, AccountDialogDisplayActions>()
                .RegisterType<ICharacterDialogActions, CharacterDialogActions>();

            //hud
            container.RegisterType<IHudButtonController, HudButtonController>()
                .RegisterType<IHudStateActions, HudStateActions>()
                .RegisterType<IStatusLabelSetter, StatusLabelSetter>();

            //map entity renderer types
            //todo: register IMapEntityRenderer implementations here
        }

        public void InitializeDependencies(IUnityContainer container)
        {
            var game = container.Resolve<IEndlessGame>();
            var gameRepository = container.Resolve<IEndlessGameRepository>();
            gameRepository.Game = game;

            gameRepository.Game.Components.Add(container.Resolve<PacketHandlerGameComponent>());

            var contentRepo = container.Resolve<IContentManagerRepository>();
            contentRepo.Content = game.Content;

            var keyboardDispatcherRepo = container.Resolve<IKeyboardDispatcherRepository>();
            keyboardDispatcherRepo.Dispatcher = new KeyboardDispatcher(game.Window);

            //part of bad hack to prevent circular dependency
            var mainButtonControllerRepo = container.Resolve<IMainButtonControllerRepository>();
            mainButtonControllerRepo.MainButtonController = container.Resolve<IMainButtonController>();

            var createAccountControllerRepo = container.Resolve<ICreateAccountControllerRepository>();
            createAccountControllerRepo.AccountController = container.Resolve<IAccountController>();

            var loginControllerRepo = container.Resolve<ILoginControllerRepository>();
            loginControllerRepo.LoginController = container.Resolve<ILoginController>();

            var charManageControllerRepo = container.Resolve<ICharacterManagementControllerRepository>();
            charManageControllerRepo.CharacterManagementController = container.Resolve<ICharacterManagementController>();
        }
    }
}
