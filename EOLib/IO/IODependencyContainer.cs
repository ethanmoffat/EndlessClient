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

            container.RegisterInstance<IConfigurationProvider, ConfigurationRepository>();
            container.RegisterInstance<IConfigurationRepository, ConfigurationRepository>();

            container.RegisterType<IPubLoadService<ItemRecord>, ItemFileLoadService>();
            container.RegisterType<IPubLoadService<NPCRecord>, NPCFileLoadService>();
            container.RegisterType<IPubLoadService<SpellRecord>, SpellFileLoadService>();
            container.RegisterType<IPubLoadService<ClassRecord>, ClassFileLoadService>();
            container.RegisterType<IMapFileLoadService, MapFileLoadService>();
            container.RegisterType<IFileRequestService, FileRequestService>();
            container.RegisterType<ILocalizedStringService, LocalizedStringService>();

            container.RegisterInstance<IPubFileRepository, PubFileRepository>();
            container.RegisterInstance<IPubFileProvider, PubFileRepository>();
            container.RegisterInstance<IItemFileRepository, PubFileRepository>();
            container.RegisterInstance<IItemFileProvider, PubFileRepository>();
            container.RegisterInstance<INPCFileRepository, PubFileRepository>();
            container.RegisterInstance<INPCFileProvider, PubFileRepository>();
            container.RegisterInstance<ISpellFileRepository, PubFileRepository>();
            container.RegisterInstance<ISpellFileProvider, PubFileRepository>();
            container.RegisterInstance<IClassFileRepository, PubFileRepository>();
            container.RegisterInstance<IClassFileProvider, PubFileRepository>();

            container.RegisterInstance<IMapFileRepository, MapFileRepository>();
            container.RegisterInstance<IMapFileProvider, MapFileRepository>();

            container.RegisterInstance<ILoginFileChecksumRepository, LoginFileChecksumRepository>();
            container.RegisterInstance<ILoginFileChecksumProvider, LoginFileChecksumRepository>();

            container.RegisterInstance<IDataFileRepository, DataFileRepository>();
            container.RegisterInstance<IDataFileProvider, DataFileRepository>();

            container.RegisterType<IFileLoadActions, FileLoadActions>();
            container.RegisterType<IFileRequestActions, FileRequestActions>();
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
