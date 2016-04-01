// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Repositories;
using EOLib.IO.Services;
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

			container.RegisterType<IPubLoadService<ItemRecord>, ItemFileLoadService>();
			container.RegisterType<IPubLoadService<NPCRecord>, NPCFileLoadService>();
			container.RegisterType<IPubLoadService<SpellRecord>, SpellFileLoadService>();
			container.RegisterType<IPubLoadService<ClassRecord>, ClassFileLoadService>();
			container.RegisterType<IMapFileLoadService, MapFileLoadService>();

			container.RegisterType<IPubFileRepository, PubFileRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IPubFileProvider, PubFileRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IItemFileRepository, PubFileRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IItemFileProvider, PubFileRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<INPCFileRepository, PubFileRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<INPCFileProvider, PubFileRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ISpellFileRepository, PubFileRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<ISpellFileProvider, PubFileRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IClassFileRepository, PubFileRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IClassFileProvider, PubFileRepository>(new ContainerControlledLifetimeManager());

			container.RegisterType<IMapFileRepository, MapFileRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IMapFileProvider, MapFileRepository>(new ContainerControlledLifetimeManager());
		}

		public void InitializeDependencies(IUnityContainer container)
		{
			var config = container.Resolve<IniReader>();
			if (!config.Load())
				throw new ConfigLoadException();
		}
	}
}
