// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Config;
using EOLib.DependencyInjection;
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
            container.RegisterType<IHDSerialNumberService, HDSerialNumberService>()
                .RegisterType<IMapFileLoadService, MapFileLoadService>()
                .RegisterType<IFileRequestService, FileRequestService>()
                .RegisterType<ILocalizedStringService, LocalizedStringService>();

            container.RegisterInstance<IConfigurationRepository, ConfigurationRepository>()
                .RegisterInstance<IConfigurationProvider, ConfigurationRepository>()
                .RegisterInstance<IMapFileRepository, MapFileRepository>()
                .RegisterInstance<IMapFileProvider, MapFileRepository>()
                .RegisterInstance<ILoginFileChecksumRepository, LoginFileChecksumRepository>()
                .RegisterInstance<ILoginFileChecksumProvider, LoginFileChecksumRepository>()
                .RegisterInstance<IDataFileRepository, DataFileRepository>()
                .RegisterInstance<IDataFileProvider, DataFileRepository>();

            container.RegisterType<IFileLoadActions, FileLoadActions>()
                .RegisterType<IFileRequestActions, FileRequestActions>()
                .RegisterType<IConfigFileLoadActions, ConfigFileLoadActions>();
        }

        public void InitializeDependencies(IUnityContainer container)
        {
            //todo: these should move to the correct assemblies (EOLib.IO and EOLib.Config)
            var fileLoadActions = container.Resolve<IFileLoadActions>();
            var configLoadActions = container.Resolve<IConfigFileLoadActions>();

            configLoadActions.LoadConfigFile();
            fileLoadActions.LoadDataFiles();
        }
    }
}
