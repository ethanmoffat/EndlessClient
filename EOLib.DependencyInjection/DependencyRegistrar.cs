// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Practices.Unity;

namespace EOLib
{
    public class DependencyRegistrar
    {
        private readonly IUnityContainer _unityContainer;

        public DependencyRegistrar(IUnityContainer unityContainer)
        {
            _unityContainer = unityContainer;
        }

        public void RegisterDependencies(params IDependencyContainer[] containers)
        {
            foreach (var container in containers)
            {
                container.RegisterDependencies(_unityContainer);
            }
        }

        public void InitializeDependencies(params IInitializableContainer[] containers)
        {
            foreach (var container in containers)
            {
                container.InitializeDependencies(_unityContainer);
            }
        }
    }
}
