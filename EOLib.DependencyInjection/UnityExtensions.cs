// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using Unity;
using Unity.Injection;
using Unity.Lifetime;

namespace EOLib.DependencyInjection
{
    public static class UnityExtensions
    {
        public static IUnityContainer RegisterInstance<T>(this IUnityContainer container)
        {
            return container.RegisterType<T>(new ContainerControlledLifetimeManager());
        }

        public static IUnityContainer RegisterInstance<T, U>(this IUnityContainer container) where U : T
        {
            if (!container.IsRegistered<U>())
                container.RegisterType<U>(new ContainerControlledLifetimeManager());

            return container.RegisterType<T, U>();
        }

        public static IUnityContainer RegisterVaried<T, U>(this IUnityContainer container) where U : T
        {
            RegisterEnumerableIfNeeded<T, U>(container);

            return container.RegisterType<T, U>(typeof(U).Name);
        }

        public static IUnityContainer RegisterInstanceVaried<T, U>(this IUnityContainer container) where U : T
        {
            RegisterEnumerableIfNeeded<T, U>(container);

            if (!container.IsRegistered<U>())
                container.RegisterType<U>(typeof(U).Name, new ContainerControlledLifetimeManager());

            return container.RegisterType<T, U>(typeof(U).Name, new ContainerControlledLifetimeManager());
        }

        private static void RegisterEnumerableIfNeeded<T, U>(IUnityContainer container) where U : T
        {
            if (!container.IsRegistered(typeof(IEnumerable<T>)))
            {
                container.RegisterType<IEnumerable<T>>(
                    new ContainerControlledLifetimeManager(),
                    new InjectionFactory(c => c.ResolveAll<T>()));
            }
        }
    }
}
