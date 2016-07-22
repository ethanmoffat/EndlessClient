// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.DependencyInjection;
using EOLib.IO.Actions;
using EOLib.IO.Repositories;
using EOLib.IO.Services;
using Microsoft.Practices.Unity;

namespace EOLib.IO
{
    public class IODependencyContainer : IDependencyContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterType<IHDSerialNumberService, HDSerialNumberService>()
                .RegisterType<IMapStringEncoderService, MapStringEncoderService>()
                .RegisterType<IMapFileLoadService, MapFileLoadService>()
                .RegisterInstance<IMapFileRepository, MapFileRepository>()
                .RegisterInstance<IMapFileProvider, MapFileRepository>()
                .RegisterType<IMapFileLoadActions, MapFileLoadActions>();
        }
    }
}
