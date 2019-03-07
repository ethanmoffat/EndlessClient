// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.DependencyInjection;
using Unity;

namespace EOLib
{
    public class EOLibDependencyContainer : IDependencyContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterType<IHDSerialNumberService, HDSerialNumberService>();
        }
    }
}
