// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using EOLib.Config;
using EOLib.IO.Actions;
using EOLib.IO.Pub;
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

            container.RegisterType<IPubLoadService<EIFRecord>, ItemFileLoadService>();
            container.RegisterType<IPubLoadService<ENFRecord>, NPCFileLoadService>();
            container.RegisterType<IPubLoadService<ESFRecord>, SpellFileLoadService>();
            container.RegisterType<IPubLoadService<ECFRecord>, ClassFileLoadService>();
            container.RegisterType<IPubFileSaveService, PubFileSaveService>();
            container.RegisterType<IMapFileLoadService, MapFileLoadService>();
            container.RegisterType<IFileRequestService, FileRequestService>();
            container.RegisterType<ILocalizedStringService, LocalizedStringService>();

            container.RegisterInstance<IPubFileRepository, PubFileRepository>();
            container.RegisterInstance<IPubFileProvider, PubFileRepository>();
            container.RegisterInstance<IEIFFileRepository, PubFileRepository>();
            container.RegisterInstance<IEIFFileProvider, PubFileRepository>();
            container.RegisterInstance<IENFFileRepository, PubFileRepository>();
            container.RegisterInstance<IENFFileProvider, PubFileRepository>();
            container.RegisterInstance<IESFFileRepository, PubFileRepository>();
            container.RegisterInstance<IESFFileProvider, PubFileRepository>();
            container.RegisterInstance<IECFFileRepository, PubFileRepository>();
            container.RegisterInstance<IECFFileProvider, PubFileRepository>();

            container.RegisterInstance<IMapFileRepository, MapFileRepository>();
            container.RegisterInstance<IMapFileProvider, MapFileRepository>();

            container.RegisterInstance<ILoginFileChecksumRepository, LoginFileChecksumRepository>();
            container.RegisterInstance<ILoginFileChecksumProvider, LoginFileChecksumRepository>();

            container.RegisterInstance<IDataFileRepository, DataFileRepository>();
            container.RegisterInstance<IDataFileProvider, DataFileRepository>();

            container.RegisterType<IFileLoadActions, FileLoadActions>()
                .RegisterType<IPubFileLoadActions, PubFileLoadActions>()
                .RegisterType<IFileRequestActions, FileRequestActions>();

            container.RegisterType<IConfigFileLoadActions, ConfigFileLoadActions>();
        }

        public void InitializeDependencies(IUnityContainer container)
        {
            //these should move to the correct assemblies (EOLib.IO and EOLib.Config)
            var pubFileLoadActions = container.Resolve<IPubFileLoadActions>();
            var fileLoadActions = container.Resolve<IFileLoadActions>();
            var configLoadActions = container.Resolve<IConfigFileLoadActions>();

            try
            {
                pubFileLoadActions.LoadItemFile();
            }
            catch (IOException)
            {
                //todo: log message?
            }

            try
            {
                pubFileLoadActions.LoadNPCFile();
            }
            catch (IOException) { }

            try
            {
                pubFileLoadActions.LoadSpellFile();
            }
            catch (IOException) { }

            try
            {
                pubFileLoadActions.LoadClassFile();
            }
            catch (IOException) { }

            configLoadActions.LoadConfigFile();
            fileLoadActions.LoadDataFiles();
        }
    }
}
