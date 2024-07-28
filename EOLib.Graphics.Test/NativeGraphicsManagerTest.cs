using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Moq;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EOLib.Graphics.Test
{
    [TestFixture, ExcludeFromCodeCoverage, Category("GraphicsDevice")]
    public class NativeGraphicsManagerTest
    {
        private GraphicsDeviceTestHelper _graphicsDeviceTestHelper;

        private INativeGraphicsLoader _graphicsLoader;
        private IGraphicsDeviceProvider _graphicsDeviceProvider;

        private bool _keepFromInfiniteLoop = false;

        private INativeGraphicsManager _nativeGraphicsManager;

        [SetUp]
        public void SetUp()
        {
            _graphicsDeviceTestHelper = new GraphicsDeviceTestHelper();

            _graphicsLoader = Mock.Of<INativeGraphicsLoader>();

            var graphicsDeviceProviderMock = new Mock<IGraphicsDeviceProvider>();
            graphicsDeviceProviderMock.Setup(x => x.GraphicsDevice)
                                      .Returns(_graphicsDeviceTestHelper.GraphicsDeviceManager.GraphicsDevice);

            _graphicsDeviceProvider = graphicsDeviceProviderMock.Object;

            _nativeGraphicsManager = new NativeGraphicsManager(_graphicsLoader, _graphicsDeviceProvider);
        }

        [TearDown]
        public void TearDown()
        {
            _nativeGraphicsManager.Dispose();
            _graphicsDeviceTestHelper.Dispose();
        }

        [Test]
        public void WhenLoadTexture_CallGraphicsLoader()
        {
            const int requestedResource = 1;

            var graphicsLoaderMock = Mock.Get(_graphicsLoader);
            LoadGFXReturnsBitmap(GFXTypes.PreLoginUI, requestedResource);
            _nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, requestedResource);

            graphicsLoaderMock.Verify(x => x.LoadGFX(GFXTypes.PreLoginUI, requestedResource), Times.Once());
        }

        [Test]
        public void WhenLoadCachedTexture_DoNotCallGraphicsLoader()
        {
            const int requestedResource = 1;

            var graphicsLoaderMock = Mock.Get(_graphicsLoader);
            LoadGFXReturnsBitmap(GFXTypes.PreLoginUI, requestedResource);
            _nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, requestedResource);
            _nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, requestedResource);

            graphicsLoaderMock.Verify(x => x.LoadGFX(GFXTypes.PreLoginUI, requestedResource), Times.Once());
        }

        [Test]
        public void WhenLoadCachedTexture_WhenReloadFromFile_CallGraphicsLoader()
        {
            const int requestedResource = 1;

            var graphicsLoaderMock = Mock.Get(_graphicsLoader);
            LoadGFXReturnsBitmap(GFXTypes.PreLoginUI, requestedResource);
            _nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, requestedResource);
            LoadGFXReturnsBitmap(GFXTypes.PreLoginUI, requestedResource);
            _nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, requestedResource, reloadFromFile: true);

            graphicsLoaderMock.Verify(x => x.LoadGFX(GFXTypes.PreLoginUI, requestedResource), Times.Exactly(2));
        }

        [Test]
        public void WhenLoadCachedTexture_WhenReloadFromFile_DisposesOriginalTextue()
        {
            const int requestedResource = 1;

            var textureHasBeenDisposed = false;

            LoadGFXReturnsBitmap(GFXTypes.PreLoginUI, requestedResource);
            var texture = _nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, requestedResource);
            texture.Disposing += (o, e) => textureHasBeenDisposed = true;

            LoadGFXReturnsBitmap(GFXTypes.PreLoginUI, requestedResource);
            _nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, requestedResource, reloadFromFile: true);

            Assert.IsTrue(textureHasBeenDisposed);
        }

        [Test]
        public void WhenLoadManyTextures_CallsGraphicsLoaderSameNumberOfTimes()
        {
            var graphicsLoaderMock = Mock.Get(_graphicsLoader);

            const int totalRequestedResources = 100;
            for (int requestedResource = 1; requestedResource <= totalRequestedResources; ++requestedResource)
            {
                LoadGFXReturnsBitmap(GFXTypes.PreLoginUI, requestedResource);
                _nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, requestedResource);

                var localRequestedResource = requestedResource;
                graphicsLoaderMock.Verify(x => x.LoadGFX(GFXTypes.PreLoginUI, localRequestedResource), Times.Once());
            }

            graphicsLoaderMock.Verify(x => x.LoadGFX(GFXTypes.PreLoginUI, It.IsAny<int>()), Times.Exactly(totalRequestedResources));
        }

        [Test]
        public void WhenLoadCachedTexture_ManyTimes_CallsGraphicsLoaderOnce()
        {
            var graphicsLoaderMock = Mock.Get(_graphicsLoader);

            const int requestedResource = 1;
            for (int i = 1; i <= 100; ++i)
            {
                LoadGFXReturnsBitmap(GFXTypes.PreLoginUI, requestedResource);
                _nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, requestedResource);
            }

            graphicsLoaderMock.Verify(x => x.LoadGFX(GFXTypes.PreLoginUI, It.IsAny<int>()), Times.Once());
        }

        [Test]
        public void WhenLoadTexture_Transparent_SetsBlackToTransparent()
        {
            const int requestedResource = 1;

            Texture2D resultTexture;
            var bmp = LoadGFXReturnsBitmap(GFXTypes.PreLoginUI, requestedResource);
            FillBitmapWithColor(bmp, Color.Black);
            resultTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, requestedResource, true);

            var data = new Microsoft.Xna.Framework.Color[resultTexture.Width * resultTexture.Height];
            resultTexture.GetData(data);

            Assert.IsTrue(data.All(x => x.A == 0));
        }

        [Test]
        public void WhenLoadTexture_MaleHat_Transparent_SetsSpecialColorToTransparent()
        {
            const int requestedResource = 1;

            Texture2D resultTexture;
            var bmp = LoadGFXReturnsBitmap(GFXTypes.MaleHat, requestedResource);
            FillBitmapWithColor(bmp, new Color(0xff000008));
            resultTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.MaleHat, requestedResource, true);

            var data = new Microsoft.Xna.Framework.Color[resultTexture.Width * resultTexture.Height];
            resultTexture.GetData(data);

            Assert.IsTrue(data.All(x => x.A == 0));
        }

        [Test]
        public void WhenLoadTexture_FemaleHat_Transparent_SetsSpecialColorToTransparent()
        {
            const int requestedResource = 1;

            Texture2D resultTexture;
            var bmp = LoadGFXReturnsBitmap(GFXTypes.FemaleHat, requestedResource);
            FillBitmapWithColor(bmp, new Color(0xff000008));
            resultTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.FemaleHat, requestedResource, true);

            var data = new Color[resultTexture.Width * resultTexture.Height];
            resultTexture.GetData(data);

            Assert.IsTrue(data.All(x => x.A == 0));
        }

        [Test]
        public void WhenLoadTexture_RaceCondition_DisposesExistingCachedTextureAndReturnsSecondOne()
        {
            const int requestedResource = 1;

            Texture2D resultTexture;
            LoadGFXReturnsBitmap(GFXTypes.PreLoginUI, requestedResource, () => GetTextureAgain(GFXTypes.PreLoginUI, requestedResource));
            resultTexture = _nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, requestedResource);

            Assert.IsFalse(resultTexture.IsDisposed);
        }

        // Manually builds a byte array that is a valid bitmap image (10x10 pixels, 32bpp)
        // Mocks the native graphics loader to return this array, and passes it back to the caller
        private Memory<byte> LoadGFXReturnsBitmap(GFXTypes whichFile, int requestedResource, Action loadCallback = null)
        {
            // 32bpp 10x10 image + 14 byte BMP header + 40 byte BITMAPINFO header
            // (4  *  100)       + 14                 + 40 = 454
            var bitmapToReturn = new byte[454];
            Array.Copy(new[] { (byte)'B', (byte)'M', }, bitmapToReturn, 2);
            Array.Copy(BitConverter.GetBytes(454), 0, bitmapToReturn, 2, 4); // total image size   [2..5]
            Array.Copy(BitConverter.GetBytes(54), 0, bitmapToReturn, 10, 4); // image data offset  [10..13]

            Array.Copy(BitConverter.GetBytes(40), 0, bitmapToReturn, 14, 4); // bitmap header size [14..17]
            Array.Copy(BitConverter.GetBytes(10), 0, bitmapToReturn, 18, 4); // width              [18..21]
            Array.Copy(BitConverter.GetBytes(10), 0, bitmapToReturn, 22, 4); // height             [22..25]
            Array.Copy(BitConverter.GetBytes((short)1), 0, bitmapToReturn, 26, 2); // planes (1)   [26..27]
            Array.Copy(BitConverter.GetBytes((short)32), 0, bitmapToReturn, 28, 2); // bpp         [28..29]

            var graphicsLoaderMock = Mock.Get(_graphicsLoader);
            graphicsLoaderMock.Setup(x => x.LoadGFX(whichFile, requestedResource))
                .Returns(bitmapToReturn)
                .Callback(loadCallback ?? (() => { }));

            return bitmapToReturn;
        }

        private static void FillBitmapWithColor(Memory<byte> image, Color color)
        {
            for (int i = 54; i < image.Length; i += 4)
            {
                image.Span[i] = color.B;
                image.Span[i + 1] = color.G;
                image.Span[i + 2] = color.R;
                image.Span[i + 3] = color.A;
            }
        }

        private void GetTextureAgain(GFXTypes whichFile, int requestedResource)
        {
            if (_keepFromInfiniteLoop) return;
            _keepFromInfiniteLoop = true;

            LoadGFXReturnsBitmap(whichFile, requestedResource);
            _nativeGraphicsManager.TextureFromResource(whichFile, requestedResource);
        }
    }
}