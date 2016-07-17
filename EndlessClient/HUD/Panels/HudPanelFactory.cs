// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering;
using EOLib.Domain.Login;
using EOLib.Graphics;

namespace EndlessClient.HUD.Panels
{
    public class HudPanelFactory : IHudPanelFactory
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IGraphicsDeviceProvider _graphicsDeviceProvider;
        private readonly INewsProvider _newsProvider;

        public HudPanelFactory(INativeGraphicsManager nativeGraphicsManager,
                               IGraphicsDeviceProvider graphicsDeviceProvider,
                               INewsProvider newsProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _graphicsDeviceProvider = graphicsDeviceProvider;
            _newsProvider = newsProvider;
        }

        public NewsPanel CreateNewsPanel()
        {
            return new NewsPanel(_nativeGraphicsManager,
                                 new ChatRenderer(_graphicsDeviceProvider, _nativeGraphicsManager),
                                 _newsProvider);
        }
    }
}