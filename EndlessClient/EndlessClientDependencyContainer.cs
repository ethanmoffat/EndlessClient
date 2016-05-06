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
using EndlessClient.Network;
using EndlessClient.Rendering.Factories;
using EndlessClient.UIControls;
using EOLib;
using Microsoft.Practices.Unity;
using XNAControls;

namespace EndlessClient
{
	public class EndlessClientDependencyContainer : IInitializableContainer
	{
		public void RegisterDependencies(IUnityContainer container)
		{
			container.RegisterType<IEndlessGame, EndlessGame>(new ContainerControlledLifetimeManager());

			//factories
			container.RegisterType<IControlSetFactory, ControlSetFactory>();
			container.RegisterType<IEOMessageBoxFactory, EOMessageBoxFactory>();
			container.RegisterType<ICreateAccountWarningDialogFactory, CreateAccountWarningDialogFactory>();
			container.RegisterType<ICreateAccountProgressDialogFactory, CreateAccountProgressDialogFactory>();
			container.RegisterType<ICharacterRendererFactory, CharacterRendererFactory>();
			container.RegisterType<ICharacterInfoPanelFactory, CharacterInfoPanelFactory>();
			container.RegisterType<ICreateCharacterDialogFactory, CreateCharacterDialogFactory>();
			container.RegisterType<IChangePasswordDialogFactory, ChangePasswordDialogFactory>();
			container.RegisterType<IGameLoadingDialogFactory, GameLoadingDialogFactory>();

			//provider/repository
			container.RegisterType<IGameStateProvider, GameStateRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IGameStateRepository, GameStateRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IContentManagerProvider, ContentManagerRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IContentManagerRepository, ContentManagerRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IKeyboardDispatcherProvider, KeyboardDispatcherRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IKeyboardDispatcherRepository, KeyboardDispatcherRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IControlSetProvider, ControlSetRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IControlSetRepository, ControlSetRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IEndlessGameProvider, EndlessGameRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IEndlessGameRepository, EndlessGameRepository>(new ContainerControlledLifetimeManager());

			//provider only
			container.RegisterType<IClientWindowSizeProvider, ClientWindowSizeProvider>(new ContainerControlledLifetimeManager());

			//controllers
			container.RegisterType<IMainButtonController, MainButtonController>();
			container.RegisterType<IAccountController, AccountController>();
			container.RegisterType<ILoginController, LoginController>();
			container.RegisterType<ICharacterManagementController, CharacterManagementController>();
			
			//controller provider/repository (bad hack - avoids circular dependency)
			container.RegisterType<IMainButtonControllerProvider, MainButtonControllerRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IMainButtonControllerRepository, MainButtonControllerRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ICreateAccountControllerProvider, CreateAccountControllerRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ICreateAccountControllerRepository, CreateAccountControllerRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ILoginControllerProvider, LoginControllerRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ILoginControllerRepository, LoginControllerRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ICharacterManagementControllerProvider, CharacterManagementRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ICharacterManagementControllerRepository, CharacterManagementRepository>(new ContainerControlledLifetimeManager());
			
			//actions
			container.RegisterType<IGameStateActions, GameStateActions>();
			container.RegisterType<IErrorDialogDisplayAction, ErrorDialogDisplayAction>();
			container.RegisterType<IAccountDialogDisplayActions, AccountDialogDisplayActions>();
			container.RegisterType<ICharacterDialogActions, CharacterDialogActions>();
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
