// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.DependencyInjection;
using Unity;

namespace EOLib.Logger
{
    public class LoggerDependencyContainer : IDependencyContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterInstance<ILoggerProvider, LoggerProvider>();
            container.RegisterType<ILoggerFactory, LoggerFactory>();
        }
    }
}
