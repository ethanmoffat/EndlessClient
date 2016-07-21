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
            container.RegisterType<IHDSerialNumberService, HDSerialNumberService>()
                .RegisterType<IPubLoadService<EIFRecord>, ItemFileLoadService>()
                .RegisterType<IPubLoadService<ENFRecord>, NPCFileLoadService>()
                .RegisterType<IPubLoadService<ESFRecord>, SpellFileLoadService>()
                .RegisterType<IPubLoadService<ECFRecord>, ClassFileLoadService>()
                .RegisterType<IPubFileSaveService, PubFileSaveService>()
                .RegisterType<IMapFileLoadService, MapFileLoadService>()
                .RegisterType<IFileRequestService, FileRequestService>()
                .RegisterType<ILocalizedStringService, LocalizedStringService>();

            container.RegisterInstance<IConfigurationRepository, ConfigurationRepository>()
                .RegisterInstance<IConfigurationProvider, ConfigurationRepository>()
                .RegisterInstance<IPubFileRepository, PubFileRepository>()
                .RegisterInstance<IPubFileProvider, PubFileRepository>()
                .RegisterInstance<IEIFFileRepository, PubFileRepository>()
                .RegisterInstance<IEIFFileProvider, PubFileRepository>()
                .RegisterInstance<IENFFileRepository, PubFileRepository>()
                .RegisterInstance<IENFFileProvider, PubFileRepository>()
                .RegisterInstance<IESFFileRepository, PubFileRepository>()
                .RegisterInstance<IESFFileProvider, PubFileRepository>()
                .RegisterInstance<IECFFileRepository, PubFileRepository>()
                .RegisterInstance<IECFFileProvider, PubFileRepository>()
                .RegisterInstance<IMapFileRepository, MapFileRepository>()
                .RegisterInstance<IMapFileProvider, MapFileRepository>()
                .RegisterInstance<ILoginFileChecksumRepository, LoginFileChecksumRepository>()
                .RegisterInstance<ILoginFileChecksumProvider, LoginFileChecksumRepository>()
                .RegisterInstance<IDataFileRepository, DataFileRepository>()
                .RegisterInstance<IDataFileProvider, DataFileRepository>();

            container.RegisterType<IFileLoadActions, FileLoadActions>()
                .RegisterType<IPubFileLoadActions, PubFileLoadActions>()
                .RegisterType<IFileRequestActions, FileRequestActions>()
                .RegisterType<IConfigFileLoadActions, ConfigFileLoadActions>();
        }

        public void InitializeDependencies(IUnityContainer container)
        {
            //todo: these should move to the correct assemblies (EOLib.IO and EOLib.Config)
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
