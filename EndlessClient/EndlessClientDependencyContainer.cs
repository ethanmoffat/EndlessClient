// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using Microsoft.Practices.Unity;

namespace EndlessClient
{
	public class EndlessClientDependencyContainer : IDependencyContainer
	{
		public void RegisterDependencies(IUnityContainer container)
		{
			container.RegisterType<IEndlessGame, EndlessGame>(new ContainerControlledLifetimeManager());
		}
	}
}
