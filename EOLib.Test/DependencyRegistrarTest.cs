// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using EOLib.DependencyInjection;
using NUnit.Framework;
using Moq;
using Unity;

namespace EOLib.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class DependencyRegistrarTest
    {
        [Test]
        public void WhenInitializeDependencies_InitializableContainers_CallsInitializeOnAll()
        {
            var containers = new[]
            {
                new Mock<IInitializableContainer>(),
                new Mock<IInitializableContainer>(),
                new Mock<IInitializableContainer>()
            };

            var unityContainer = Mock.Of<IUnityContainer>();
            var registrar = new DependencyRegistrar(unityContainer);

            registrar.InitializeDependencies(containers.Select(x => x.Object).ToArray());

            foreach (var container in containers)
                container.Verify(x => x.InitializeDependencies(unityContainer), Times.Once());
        }

        [Test]
        public void WhenRegisterDependencies_DependencyContainers_CallsRegisterOnAll()
        {
            var containers = new[]
            {
                new Mock<IDependencyContainer>(),
                new Mock<IDependencyContainer>(),
                new Mock<IDependencyContainer>()
            };

            var unityContainer = Mock.Of<IUnityContainer>();
            var registrar = new DependencyRegistrar(unityContainer);

            registrar.RegisterDependencies(containers.Select(x => x.Object).ToArray());

            foreach (var container in containers)
                container.Verify(x => x.RegisterDependencies(unityContainer), Times.Once());
        }

        [Test]
        public void WhenRegisterDependencies_MixedTypes_CallsRegisterOnAll()
        {
            var containers = new[]
            {
                new Mock<IDependencyContainer>(),
                new Mock<IDependencyContainer>(),
                new Mock<IDependencyContainer>()
            };
            var containers2 = new[]
            {
                new Mock<IInitializableContainer>(),
                new Mock<IInitializableContainer>(),
                new Mock<IInitializableContainer>()
            };

            var unityContainer = Mock.Of<IUnityContainer>();
            var registrar = new DependencyRegistrar(unityContainer);

            registrar.RegisterDependencies(containers
                .Select(x => x.Object)
                .Concat(containers2.Select(x => x.Object))
                .ToArray());

            foreach (var container in containers)
                container.Verify(x => x.RegisterDependencies(unityContainer), Times.Once());
            foreach (var container in containers2)
                container.Verify(x => x.RegisterDependencies(unityContainer), Times.Once());
        }
    }
}
