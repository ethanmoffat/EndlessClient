// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PELoaderLib;

namespace EOLib.Graphics.Test
{
    [TestClass, ExcludeFromCodeCoverage]
    public class NativeGraphicsLoaderTest
    {
        private IPEFileCollection _modules;
        private INativeGraphicsLoader _nativeGraphicsLoader;

        [TestInitialize]
        public void TestInitialize()
        {
            _modules = Mock.Of<IPEFileCollection>();

            _nativeGraphicsLoader = new NativeGraphicsLoader(_modules);
        }

        [TestMethod]
        public void WhenLoadGFX_CallsPEFile_GetEmbeddedBitmapResourceByID()
        {
            var peFileMock = SetupPEFileForGFXType(GFXTypes.PreLoginUI, CreateDataArrayForBitmap());

            using (var bmp = _nativeGraphicsLoader.LoadGFX(GFXTypes.PreLoginUI, 1))
                bmp.Dispose(); //hide warning for empty using statement

            peFileMock.Verify(x => x.GetEmbeddedBitmapResourceByID(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
        }

        [TestMethod]
        public void WhenLoadGFX_CallsPEFile_WithResourceValueIncreasedBy100()
        {
            const int requestedResourceID = 1;
            const int expectedResourceID = 101;

            var peFileMock = SetupPEFileForGFXType(GFXTypes.PreLoginUI, CreateDataArrayForBitmap());

            using (var bmp = _nativeGraphicsLoader.LoadGFX(GFXTypes.PreLoginUI, requestedResourceID))
                bmp.Dispose(); //hide warning for empty using statement

            peFileMock.Verify(x => x.GetEmbeddedBitmapResourceByID(expectedResourceID, It.IsAny<int>()));
        }

        [TestMethod, ExpectedException(typeof (GFXLoadException))]
        public void WhenLoadGFX_EmptyDataArray_ThrowsException()
        {
            const int requestedResourceID = 1;

            SetupPEFileForGFXType(GFXTypes.PreLoginUI, new byte[] { });

            _nativeGraphicsLoader.LoadGFX(GFXTypes.PreLoginUI, requestedResourceID);
        }

        private byte[] CreateDataArrayForBitmap()
        {
            byte[] array;
            using (var retBmp = new Bitmap(10, 10))
            {
                using (var ms = new MemoryStream())
                {
                    retBmp.Save(ms, ImageFormat.Bmp);
                    array = ms.ToArray();
                }
            }
            return array;
        }

        private Mock<IPEFile> SetupPEFileForGFXType(GFXTypes type, byte[] array)
        {
            var collectionMock = Mock.Get(_modules);
            var peFile = new Mock<IPEFile>();
            collectionMock.Setup(x => x[type]).Returns(peFile.Object);

            peFile.Setup(x => x.GetEmbeddedBitmapResourceByID(It.IsAny<int>(), It.IsAny<int>()))
                  .Returns(array);

            return peFile;
        }
    }
}
