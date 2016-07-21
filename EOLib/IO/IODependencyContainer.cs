// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.DependencyInjection;
using EOLib.IO.Actions;
using EOLib.IO.Repositories;
using EOLib.IO.Services;
using EOLib.Localization;
using Microsoft.Practices.Unity;

namespace EOLib.IO
{
    public class IODependencyContainer : IDependencyContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterType<IHDSerialNumberService, HDSerialNumberService>()
                .RegisterType<IMapFileLoadService, MapFileLoadService>()
                .RegisterType<IFileRequestService, FileRequestService>();

            container
                .RegisterInstance<IMapFileRepository, MapFileRepository>()
                .RegisterInstance<IMapFileProvider, MapFileRepository>()
                .RegisterInstance<ILoginFileChecksumRepository, LoginFileChecksumRepository>()
                .RegisterInstance<ILoginFileChecksumProvider, LoginFileChecksumRepository>();

            container.RegisterType<IFileLoadActions, FileLoadActions>()
                .RegisterType<IFileRequestActions, FileRequestActions>();
        }
    }
}
