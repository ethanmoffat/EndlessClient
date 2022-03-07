using System.Diagnostics.CodeAnalysis;
using System.IO;
using NUnit.Framework;
using Moq;
using PELoaderLib;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Bmp;
using EOLib.Config;

namespace EOLib.Graphics.Test
{
    [TestFixture, ExcludeFromCodeCoverage, Category("GraphicsDevice")]
    public class NativeGraphicsLoaderTest
    {
        private IPEFileCollection _modules;
        private INativeGraphicsLoader _nativeGraphicsLoader;

        private const int ExpectedCulture = 0;
        private const BitmapVersion ExpectedBitmapVersion = BitmapVersion.BitmapInfoHeader;

        [SetUp]
        public void SetUp()
        {
            _modules = Mock.Of<IPEFileCollection>();
            _nativeGraphicsLoader = new NativeGraphicsLoader(_modules, Mock.Of<IConfigurationProvider>(x => x.MainCloneCompat == false));
        }

        [Test]
        public void WhenLoadGFX_CallsPEFile_GetEmbeddedBitmapResourceByID()
        {
            var peFileMock = SetupPEFileForGFXType(GFXTypes.PreLoginUI, CreateDataArrayForBitmap());

            using (var bmp = _nativeGraphicsLoader.LoadGFX(GFXTypes.PreLoginUI, 1))
                bmp.Dispose(); //hide warning for empty using statement

            peFileMock.Verify(x => x.GetEmbeddedBitmapResourceByID(It.IsAny<int>(), ExpectedBitmapVersion, ExpectedCulture), Times.Once());
        }

        [Test]
        public void WhenLoadGFX_CallsPEFile_WithResourceValueIncreasedBy100()
        {
            const int requestedResourceID = 1;
            const int expectedResourceID = 101;

            var peFileMock = SetupPEFileForGFXType(GFXTypes.PreLoginUI, CreateDataArrayForBitmap());

            using (var bmp = _nativeGraphicsLoader.LoadGFX(GFXTypes.PreLoginUI, requestedResourceID))
                bmp.Dispose(); //hide warning for empty using statement

            peFileMock.Verify(x => x.GetEmbeddedBitmapResourceByID(expectedResourceID, ExpectedBitmapVersion, ExpectedCulture));
        }

        [Test]
        public void WhenLoadGFX_EmptyDataArray_ThrowsException()
        {
            const int requestedResourceID = 1;

            SetupPEFileForGFXType(GFXTypes.PreLoginUI, new byte[] { });

            Assert.Throws<GFXLoadException>(() => _nativeGraphicsLoader.LoadGFX(GFXTypes.PreLoginUI, requestedResourceID));
        }

        private byte[] CreateDataArrayForBitmap()
        {
            byte[] array;
            using (var retBmp = new Image<Rgb24>(10, 10))
            {
                using (var ms = new MemoryStream())
                {
                    retBmp.Save(ms, new BmpEncoder());
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

            peFile.Setup(x => x.GetEmbeddedBitmapResourceByID(It.IsAny<int>(), ExpectedBitmapVersion, ExpectedCulture))
                  .Returns(array);

            return peFile;
        }
    }
}
