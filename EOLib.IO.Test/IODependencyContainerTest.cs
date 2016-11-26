// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using EOLib.IO.Actions;
using EOLib.Logger;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EOLib.IO.Test
{
    [TestClass, ExcludeFromCodeCoverage]
    public class IODependencyContainerTest
    {
        [TestMethod]
        public void RegisterDependencies_RegistersTypes()
        {
            var container = new IODependencyContainer();
            var unityContainer = new UnityContainer();

            container.RegisterDependencies(unityContainer);

            Assert.AreNotEqual(0, unityContainer.Registrations.Count());
        }

        [TestMethod]
        public void InitializeDependencies_LoadsPubFiles()
        {
            IUnityContainer unityContainer = new UnityContainer();
            SetupUnityContainer(unityContainer, new Mock<IPubFileLoadActions>());

            new IODependencyContainer().InitializeDependencies(unityContainer);

            var actionsMock = Mock.Get(unityContainer.Resolve<IPubFileLoadActions>());
            actionsMock.Verify(x => x.LoadItemFile(), Times.Once);
            actionsMock.Verify(x => x.LoadNPCFile(), Times.Once);
            actionsMock.Verify(x => x.LoadSpellFile(), Times.Once);
            actionsMock.Verify(x => x.LoadClassFile(), Times.Once);
        }

        [TestMethod]
        public void InitializeDependencies_NoItemFile_LogsMessageAndContinuesLoad()
        {
            var pubFileLoadActionsMock = new Mock<IPubFileLoadActions>();
            pubFileLoadActionsMock.Setup(x => x.LoadItemFile()).Throws<IOException>();

            var unityContainer = new UnityContainer();
            SetupUnityContainer(unityContainer, pubFileLoadActionsMock);

            new IODependencyContainer().InitializeDependencies(unityContainer);

            var actionsMock = Mock.Get(unityContainer.Resolve<IPubFileLoadActions>());
            actionsMock.Verify(x => x.LoadItemFile(), Times.Once);
            actionsMock.Verify(x => x.LoadNPCFile(), Times.Once);
            actionsMock.Verify(x => x.LoadSpellFile(), Times.Once);
            actionsMock.Verify(x => x.LoadClassFile(), Times.Once);

            var loggerMock = Mock.Get(unityContainer.Resolve<ILoggerProvider>().Logger);
            loggerMock.Verify(x => x.Log(It.IsAny<string>(), PubFileNameConstants.PathToEIFFile, It.IsAny<string>()));
        }

        [TestMethod]
        public void InitializeDependencies_NoNPCFile_LogsMessageAndContinuesLoad()
        {
            var pubFileLoadActionsMock = new Mock<IPubFileLoadActions>();
            pubFileLoadActionsMock.Setup(x => x.LoadNPCFile()).Throws<IOException>();

            var unityContainer = new UnityContainer();
            SetupUnityContainer(unityContainer, pubFileLoadActionsMock);

            new IODependencyContainer().InitializeDependencies(unityContainer);

            var actionsMock = Mock.Get(unityContainer.Resolve<IPubFileLoadActions>());
            actionsMock.Verify(x => x.LoadItemFile(), Times.Once);
            actionsMock.Verify(x => x.LoadNPCFile(), Times.Once);
            actionsMock.Verify(x => x.LoadSpellFile(), Times.Once);
            actionsMock.Verify(x => x.LoadClassFile(), Times.Once);

            var loggerMock = Mock.Get(unityContainer.Resolve<ILoggerProvider>().Logger);
            loggerMock.Verify(x => x.Log(It.IsAny<string>(), PubFileNameConstants.PathToENFFile, It.IsAny<string>()));
        }

        [TestMethod]
        public void InitializeDependencies_NoSpellFile_LogsMessageAndContinuesLoad()
        {
            var pubFileLoadActionsMock = new Mock<IPubFileLoadActions>();
            pubFileLoadActionsMock.Setup(x => x.LoadSpellFile()).Throws<IOException>();

            var unityContainer = new UnityContainer();
            SetupUnityContainer(unityContainer, pubFileLoadActionsMock);

            new IODependencyContainer().InitializeDependencies(unityContainer);

            var actionsMock = Mock.Get(unityContainer.Resolve<IPubFileLoadActions>());
            actionsMock.Verify(x => x.LoadItemFile(), Times.Once);
            actionsMock.Verify(x => x.LoadNPCFile(), Times.Once);
            actionsMock.Verify(x => x.LoadSpellFile(), Times.Once);
            actionsMock.Verify(x => x.LoadClassFile(), Times.Once);

            var loggerMock = Mock.Get(unityContainer.Resolve<ILoggerProvider>().Logger);
            loggerMock.Verify(x => x.Log(It.IsAny<string>(), PubFileNameConstants.PathToESFFile, It.IsAny<string>()));
        }

        [TestMethod]
        public void InitializeDependencies_NoClassFile_LogsMessageAndContinuesLoad()
        {
            var pubFileLoadActionsMock = new Mock<IPubFileLoadActions>();
            pubFileLoadActionsMock.Setup(x => x.LoadClassFile()).Throws<IOException>();

            var unityContainer = new UnityContainer();
            SetupUnityContainer(unityContainer, pubFileLoadActionsMock);

            new IODependencyContainer().InitializeDependencies(unityContainer);

            var actionsMock = Mock.Get(unityContainer.Resolve<IPubFileLoadActions>());
            actionsMock.Verify(x => x.LoadItemFile(), Times.Once);
            actionsMock.Verify(x => x.LoadNPCFile(), Times.Once);
            actionsMock.Verify(x => x.LoadSpellFile(), Times.Once);
            actionsMock.Verify(x => x.LoadClassFile(), Times.Once);

            var loggerMock = Mock.Get(unityContainer.Resolve<ILoggerProvider>().Logger);
            loggerMock.Verify(x => x.Log(It.IsAny<string>(), PubFileNameConstants.PathToECFFile, It.IsAny<string>()));
        }

        private static void SetupUnityContainer(IUnityContainer unityContainer, Mock<IPubFileLoadActions> pubFileLoadActionsMock)
        {
            unityContainer.RegisterType<IPubFileLoadActions>(new ContainerControlledLifetimeManager(),
                                                             new InjectionFactory(c => pubFileLoadActionsMock.Object));
            unityContainer.RegisterType<ILoggerProvider>(new ContainerControlledLifetimeManager(),
                                                         new InjectionFactory(c => Mock.Of<ILoggerProvider>(x => x.Logger == Mock.Of<ILogger>())));
        }
    }
}
