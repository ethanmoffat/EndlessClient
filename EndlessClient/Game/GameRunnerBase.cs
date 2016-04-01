// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using EOLib.Data;
using EOLib.IO;
using EOLib.Net;
using Microsoft.Practices.Unity;

namespace EndlessClient.Game
{
	public abstract class GameRunnerBase : IGameRunner
	{
		private readonly IUnityContainer _unityContainer;

		protected GameRunnerBase(IUnityContainer unityContainer)
		{
			_unityContainer = unityContainer;
		}

		public virtual bool SetupDependencies()
		{
			var registrar = new DependencyRegistrar(_unityContainer);

			registrar.RegisterDependencies(new DataDependencyContainer(),
				new IODependencyContainer(),
				new NetworkDependencyContainer(),
				new EndlessClientDependencyContainer());

			registrar.InitializeDependencies(new NetworkDependencyContainer());

			return true;
		}

		public virtual void RunGame()
		{
			var game = _unityContainer.Resolve<IEndlessGame>();
			game.Run();
		}
	}
}
