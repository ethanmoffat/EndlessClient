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
using EndlessClient.Input;
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
			container.RegisterInstance<IEndlessGame, EndlessGame>();

			//factories
			container.RegisterType<IControlSetFactory, ControlSetFactory>();
			container.RegisterType<IHudControlsFactory, HudControlsFactory>();
			container.RegisterType<ICharacterRendererFactory, CharacterRendererFactory>();
			container.RegisterType<ICharacterInfoPanelFactory, CharacterInfoPanelFactory>();

			container.RegisterType<IEOMessageBoxFactory, EOMessageBoxFactory>();
			container.RegisterType<ICreateAccountWarningDialogFactory, CreateAccountWarningDialogFactory>();
			container.RegisterType<ICreateAccountProgressDialogFactory, CreateAccountProgressDialogFactory>();
			container.RegisterType<ICreateCharacterDialogFactory, CreateCharacterDialogFactory>();
			container.RegisterType<IChangePasswordDialogFactory, ChangePasswordDialogFactory>();
			container.RegisterType<IGameLoadingDialogFactory, GameLoadingDialogFactory>();

			//provider/repository
			container.RegisterInstance<IGameStateProvider, GameStateRepository>();
			container.RegisterInstance<IGameStateRepository, GameStateRepository>();
			container.RegisterInstance<IContentManagerProvider, ContentManagerRepository>();
			container.RegisterInstance<IContentManagerRepository, ContentManagerRepository>();
			container.RegisterInstance<IKeyboardDispatcherProvider, KeyboardDispatcherRepository>();
			container.RegisterInstance<IKeyboardDispatcherRepository, KeyboardDispatcherRepository>();
			container.RegisterInstance<IControlSetProvider, ControlSetRepository>();
			container.RegisterInstance<IControlSetRepository, ControlSetRepository>();
			container.RegisterInstance<IEndlessGameProvider, EndlessGameRepository>();
			container.RegisterInstance<IEndlessGameRepository, EndlessGameRepository>();

			//provider only
			container.RegisterInstance<IClientWindowSizeProvider, ClientWindowSizeProvider>();

			//controllers
			container.RegisterType<IMainButtonController, MainButtonController>();
			container.RegisterType<IAccountController, AccountController>();
			container.RegisterType<ILoginController, LoginController>();
			container.RegisterType<ICharacterManagementController, CharacterManagementController>();
			
			//controller provider/repository (bad hack - avoids circular dependency)
			container.RegisterInstance<IMainButtonControllerProvider, MainButtonControllerRepository>();
			container.RegisterInstance<IMainButtonControllerRepository, MainButtonControllerRepository>();
			container.RegisterInstance<ICreateAccountControllerProvider, CreateAccountControllerRepository>();
			container.RegisterInstance<ICreateAccountControllerRepository, CreateAccountControllerRepository>();
			container.RegisterInstance<ILoginControllerProvider, LoginControllerRepository>();
			container.RegisterInstance<ILoginControllerRepository, LoginControllerRepository>();
			container.RegisterInstance<ICharacterManagementControllerProvider, CharacterManagementRepository>();
			container.RegisterInstance<ICharacterManagementControllerRepository, CharacterManagementRepository>();
			
			//actions
			container.RegisterType<IGameStateActions, GameStateActions>();
			container.RegisterType<IErrorDialogDisplayAction, ErrorDialogDisplayAction>();
			container.RegisterType<IAccountDialogDisplayActions, AccountDialogDisplayActions>();
			container.RegisterType<ICharacterDialogActions, CharacterDialogActions>();

			//hud
			container.RegisterType<IHudButtonController, HudButtonController>();
			container.RegisterType<IHudStateActions, HudStateActions>();
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
