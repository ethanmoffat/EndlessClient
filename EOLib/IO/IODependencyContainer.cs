// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.DependencyInjection;
using Microsoft.Practices.Unity;

namespace EOLib.IO
{
    public class IODependencyContainer : IDependencyContainer
    {
        public void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterType<IHDSerialNumberService, HDSerialNumberService>();
        }
    }
}
