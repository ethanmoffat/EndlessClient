// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Practices.Unity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EOLib.Localization.Test
{
    [TestClass, ExcludeFromCodeCoverage]
    public class LocalizationDependencyContainerTest
    {
        [TestMethod]
        public void LocalizationDependencyContainer_Register_DoesRegistration()
        {
            var container = new LocalizationDependencyContainer();
            var unityContainer = new UnityContainer();
            container.RegisterDependencies(unityContainer);

            Assert.AreNotEqual(0, unityContainer.Registrations.Count());
        }

        [TestMethod]
        public void LocalizationDependencyContainer_Initialize_LoadsEDFFiles()
        {
            var dataFileLoadActions = new Mock<IDataFileLoadActions>();
            var unityContainer = new UnityContainer();
            unityContainer.RegisterInstance(dataFileLoadActions.Object);

            var container = new LocalizationDependencyContainer();
            container.InitializeDependencies(unityContainer);

            dataFileLoadActions.Verify(x => x.LoadDataFiles(), Times.Once);
        }
    }
}
