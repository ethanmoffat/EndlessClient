// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using EOLib.IO.Actions;
using EOLib.IO.Repositories;
using EOLib.IO.Services;
using Microsoft.Practices.Unity;

namespace EOLib.IO
{
	public class IODependencyContainer : IInitializableContainer
	{
		public void RegisterDependencies(IUnityContainer container)
		{
			container.RegisterType<IHDSerialNumberService, HDSerialNumberService>();

			container.RegisterType<IConfigurationProvider, ConfigurationRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IConfigurationRepository, ConfigurationRepository>(new ContainerControlledLifetimeManager());

			container.RegisterType<IPubLoadService<ItemRecord>, ItemFileLoadService>();
			container.RegisterType<IPubLoadService<NPCRecord>, NPCFileLoadService>();
			container.RegisterType<IPubLoadService<SpellRecord>, SpellFileLoadService>();
			container.RegisterType<IPubLoadService<ClassRecord>, ClassFileLoadService>();
			container.RegisterType<IMapFileLoadService, MapFileLoadService>();
			container.RegisterType<ILocalizedStringService, LocalizedStringService>();

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

			container.RegisterType<IDataFileRepository, DataFileRepository>(new ContainerControlledLifetimeManager());
			container.RegisterType<IDataFileProvider, DataFileRepository>(new ContainerControlledLifetimeManager());

			container.RegisterType<IFileLoadActions, FileLoadActions>();
		}

		public void InitializeDependencies(IUnityContainer container)
		{
			var fileLoadActions = container.Resolve<IFileLoadActions>();

			try
			{
				fileLoadActions.LoadItemFile();
			}
			catch (IOException)
			{
				//todo: log message?
			}

			try
			{
				fileLoadActions.LoadNPCFile();
			}
			catch (IOException) { }

			try
			{
				fileLoadActions.LoadSpellFile();
			}
			catch (IOException) { }

			try
			{
				fileLoadActions.LoadClassFile();
			}
			catch (IOException) { }

			fileLoadActions.LoadConfigFile();
			fileLoadActions.LoadDataFiles();
		}
	}
}
