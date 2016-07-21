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

            container
                .RegisterInstance<IMapFileRepository, MapFileRepository>()
                .RegisterInstance<IMapFileProvider, MapFileRepository>()
                .RegisterInstance<ILoginFileChecksumRepository, LoginFileChecksumRepository>()
                .RegisterInstance<ILoginFileChecksumProvider, LoginFileChecksumRepository>()
                .RegisterInstance<IDataFileRepository, DataFileRepository>()
                .RegisterInstance<IDataFileProvider, DataFileRepository>();

            container.RegisterType<IFileLoadActions, FileLoadActions>()
                .RegisterType<IFileRequestActions, FileRequestActions>();
        }

        public void InitializeDependencies(IUnityContainer container)
        {
            var fileLoadActions = container.Resolve<IFileLoadActions>();

            fileLoadActions.LoadDataFiles();
        }
    }
}
