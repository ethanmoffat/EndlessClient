// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Config;
using Microsoft.Practices.Unity;

namespace EOLib.IO
{
	public class IODependencyContainer : IDependencyContainer, IInitializableContainer
	{
		public void RegisterDependencies(IUnityContainer container)
		{
			container.RegisterType<IHDSerialNumberService, HDSerialNumberService>();

			container.RegisterType<IClientVersionProvider, ClientVersionProvider>(new ContainerControlledLifetimeManager());

			container.RegisterType<IConfigurationProvider, ConfigurationRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IConfigurationRepository, ConfigurationRepository>(new ContainerControlledLifetimeManager());

			container.RegisterType<IniReader>(
				new ContainerControlledLifetimeManager(),
				new InjectionFactory(c => new IniReader(ConfigStrings.Default_Config_File)));
		}

		public void InitializeDependencies(IUnityContainer container)
		{
			var config = container.Resolve<IniReader>();
			if (!config.Load())
				throw new ConfigLoadException();
		}
	}
}
