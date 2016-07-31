// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Factories
{
    public class RenderTargetFactory : IRenderTargetFactory
    {
        private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;

        public RenderTargetFactory(IGraphicsDeviceProvider graphicsDeviceProvider,
                                      IClientWindowSizeProvider clientWindowSizeProvider)
        {
            _graphicsDeviceProvider = graphicsDeviceProvider;
            _clientWindowSizeProvider = clientWindowSizeProvider;
        }

        public RenderTarget2D CreateRenderTarget()
        {
            return new RenderTarget2D(
                _graphicsDeviceProvider.GraphicsDevice,
                _clientWindowSizeProvider.Width,
                _clientWindowSizeProvider.Height,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);
        }
    }
}
