// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NUnit.Framework;
using Unity;

namespace EOLib.IO.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class IODependencyContainerTest
    {
        [Test]
        public void RegisterDependencies_RegistersTypes()
        {
            var container = new IODependencyContainer();
            IUnityContainer unityContainer = new UnityContainer();

            container.RegisterDependencies(unityContainer);

            Assert.AreNotEqual(0, unityContainer.Registrations.Count());
        }
    }
}
