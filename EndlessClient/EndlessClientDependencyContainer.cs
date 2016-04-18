// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Content;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Actions;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
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

			//provider/repository
			container.RegisterType<IGameStateProvider, GameStateRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IGameStateRepository, GameStateRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IContentManagerProvider, ContentManagerRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IContentManagerRepository, ContentManagerRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IKeyboardDispatcherProvider, KeyboardDispatcherRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IKeyboardDispatcherRepository, KeyboardDispatcherRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IControlSetProvider, ControlSetRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IControlSetRepository, ControlSetRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IMainButtonControllerProvider, MainButtonControllerRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IMainButtonControllerRepository, MainButtonControllerRepository>(new ContainerControlledLifetimeManager());

			//provider only
			container.RegisterType<IClientWindowSizeProvider, ClientWindowSizeProvider>(new ContainerControlledLifetimeManager());

			//controllers
			container.RegisterType<IMainButtonController, MainButtonController>();
			
			//actions
			container.RegisterType<IGameStateActions, GameStateActions>();
			container.RegisterType<IErrorDialogDisplayAction, ErrorDialogDisplayAction>();
			container.RegisterType<ICreateAccountDialogDisplayAction, CreateAccountDialogDisplayAction>();
		}

		public void InitializeDependencies(IUnityContainer container)
		{
			var game = container.Resolve<IEndlessGame>();
			var contentRepo = container.Resolve<IContentManagerRepository>();
			contentRepo.Content = game.Content;

			var keyboardDispatcherRepo = container.Resolve<IKeyboardDispatcherRepository>();
			keyboardDispatcherRepo.Dispatcher = new KeyboardDispatcher(game.Window);

			var mainButtonControllerRepo = container.Resolve<IMainButtonControllerRepository>();
			mainButtonControllerRepo.MainButtonController = container.Resolve<IMainButtonController>();
		}
	}
}
