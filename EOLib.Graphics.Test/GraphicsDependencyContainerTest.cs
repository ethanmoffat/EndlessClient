// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Moq;
using PELoaderLib;
using Unity;
using Unity.Lifetime;

namespace EOLib.Graphics.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class GraphicsDependencyContainerTest
    {
        private Dictionary<GFXTypes, IPEFile> _gfxFiles;

        [SetUp]
        public void SetUp()
        {
            _gfxFiles = new Dictionary<GFXTypes, IPEFile>();
        }

        [Test]
        public void RegistersDependencies_DoesRegistrations()
        {
            IUnityContainer unityContainer = new UnityContainer();
            var container = new GraphicsDependencyContainer();

            container.RegisterDependencies(unityContainer);

            Assert.AreNotEqual(0, unityContainer.Registrations.Count());
        }

        [Test]
        public void InitializeDependencies_PEFileError_ExpectIOExceptionIsThrownAsLibraryLoadException()
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterFactory<IPEFileCollection>(c => CreatePEFileCollection());
            var container = new GraphicsDependencyContainer();

            var file1Mock = new Mock<IPEFile>();
            file1Mock.Setup(x => x.Initialize()).Throws<IOException>();
            _gfxFiles.Add(GFXTypes.PreLoginUI, file1Mock.Object);

            Assert.Throws<LibraryLoadException>(() => container.InitializeDependencies(unityContainer));
        }

        [Test]
        public void InitializeDependencies_PEFileInitializeIsFalse_ExpectLibraryLoadException()
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterFactory<IPEFileCollection>(c => CreatePEFileCollection());
            var container = new GraphicsDependencyContainer();

            var file1Mock = new Mock<IPEFile>();
            file1Mock.Setup(x => x.Initialized).Returns(false);
            _gfxFiles.Add(GFXTypes.PreLoginUI, file1Mock.Object);

            Assert.Throws<LibraryLoadException>(() => container.InitializeDependencies(unityContainer));
        }

        [Test]
        public void InitializeDependencies_InitializesGFXFiles()
        {
            var unityContainer = new UnityContainer();
            unityContainer.RegisterFactory<IPEFileCollection>(c => CreatePEFileCollection(), new ContainerControlledLifetimeManager());
            var container = new GraphicsDependencyContainer();

            var file1Mock = new Mock<IPEFile>();
            file1Mock.Setup(x => x.Initialized).Returns(true);
            var file2Mock = new Mock<IPEFile>();
            file2Mock.Setup(x => x.Initialized).Returns(true);
            var file3Mock = new Mock<IPEFile>();
            file3Mock.Setup(x => x.Initialized).Returns(true);

            _gfxFiles.Add(GFXTypes.PreLoginUI, file1Mock.Object);
            _gfxFiles.Add(GFXTypes.PostLoginUI, file2Mock.Object);
            _gfxFiles.Add(GFXTypes.MapTiles, file3Mock.Object);

            container.InitializeDependencies(unityContainer);

            Mock.Get(unityContainer.Resolve<IPEFileCollection>())
                .Verify(x => x.PopulateCollectionWithStandardGFX(), Times.Once);
            file1Mock.Verify(x => x.Initialize(), Times.Once);
            file2Mock.Verify(x => x.Initialize(), Times.Once);
            file3Mock.Verify(x => x.Initialize(), Times.Once);
        }

        private IPEFileCollection CreatePEFileCollection()
        {
            var collection = new Mock<IPEFileCollection>();
            collection.Setup(x => x.GetEnumerator()).Returns(_gfxFiles.GetEnumerator());
            return collection.Object;
        }
    }
}
