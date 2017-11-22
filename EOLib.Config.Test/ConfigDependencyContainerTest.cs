// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Moq;

namespace EOLib.Config.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class ConfigDependencyContainerTest
    {
        [Test]
        public void RegisterDependencies_RegistersTypes()
        {
            var container = new ConfigDependencyContainer();
            var unityContainer = new UnityContainer();

            container.RegisterDependencies(unityContainer);

            Assert.AreNotEqual(0, unityContainer.Registrations.Count());
        }

        [Test]
        public void InitializeDependencies_LoadsConfigFile()
        {
            var container = new ConfigDependencyContainer();
            var unityContainer = new UnityContainer();
            unityContainer.RegisterType<IConfigFileLoadActions>(
                new ContainerControlledLifetimeManager(),
                new InjectionFactory(CreateConfigLoadActions));

            container.InitializeDependencies(unityContainer);

            Mock.Get(unityContainer.Resolve<IConfigFileLoadActions>())
                .Verify(x => x.LoadConfigFile(), Times.Once);
        }

        private IConfigFileLoadActions CreateConfigLoadActions(IUnityContainer arg)
        {
            return Mock.Of<IConfigFileLoadActions>();
        }
    }
}
