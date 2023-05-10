using Moq;
using NUnit.Framework;
using PELoaderLib;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace EOLib.Graphics.Test
{
    [TestFixture, ExcludeFromCodeCoverage, Category("GraphicsDevice")]
    public class NativeGraphicsLoaderTest
    {
        private IPEFileCollection _modules;
        private INativeGraphicsLoader _nativeGraphicsLoader;

        private const int ExpectedCulture = -1;

        [SetUp]
        public void SetUp()
        {
            _modules = Mock.Of<IPEFileCollection>();
            _nativeGraphicsLoader = new NativeGraphicsLoader(_modules);
        }

        [Test]
        public void WhenLoadGFX_CallsPEFile_GetEmbeddedBitmapResourceByID()
        {
            var peFileMock = SetupPEFileForGFXType(GFXTypes.PreLoginUI, CreateDataArrayForBitmap());

            var data = _nativeGraphicsLoader.LoadGFX(GFXTypes.PreLoginUI, 1);
            peFileMock.Verify(x => x.GetEmbeddedBitmapResourceByID(It.IsAny<int>(), ExpectedCulture), Times.Once());
        }

        [Test]
        public void WhenLoadGFX_CallsPEFile_WithResourceValueIncreasedBy100()
        {
            const int requestedResourceID = 1;
            const int expectedResourceID = 101;

            var peFileMock = SetupPEFileForGFXType(GFXTypes.PreLoginUI, CreateDataArrayForBitmap());

            var data = _nativeGraphicsLoader.LoadGFX(GFXTypes.PreLoginUI, requestedResourceID);
            peFileMock.Verify(x => x.GetEmbeddedBitmapResourceByID(expectedResourceID, ExpectedCulture));
        }

        [Test]
        public void WhenLoadGFX_EmptyDataArray_ThrowsInDebug_EmptyInRelease()
        {
            const int requestedResourceID = 1;

            SetupPEFileForGFXType(GFXTypes.PreLoginUI, new byte[] { });

#if DEBUG
            Assert.Throws<GFXLoadException>(() => _nativeGraphicsLoader.LoadGFX(GFXTypes.PreLoginUI, requestedResourceID));
#else
            Assert.That(_nativeGraphicsLoader.LoadGFX(GFXTypes.PreLoginUI, requestedResourceID), Has.Length.EqualTo(0));
#endif
        }

        private byte[] CreateDataArrayForBitmap()
        {
            return new byte[1];
        }

        private Mock<IPEFile> SetupPEFileForGFXType(GFXTypes type, byte[] array)
        {
            var collectionMock = Mock.Get(_modules);
            var peFile = new Mock<IPEFile>();
            collectionMock.Setup(x => x[type]).Returns(peFile.Object);

            peFile.Setup(x => x.GetEmbeddedBitmapResourceByID(It.IsAny<int>(), ExpectedCulture))
                  .Returns(array);

            return peFile;
        }
    }
}
