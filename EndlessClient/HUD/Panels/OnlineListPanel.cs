// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Graphics;
using Microsoft.Xna.Framework;
using XNAControls.Old;

namespace EndlessClient.HUD.Panels
{
    public class OnlineListPanel : XNAPanel, IHudPanel
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;

        public OnlineListPanel(INativeGraphicsManager nativeGraphicsManager)
            : base(new Rectangle(102, 330, 1, 1))
        {
            _nativeGraphicsManager = nativeGraphicsManager;

            BackgroundImage = _nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 36);
            _setSize(BackgroundImage.Width, BackgroundImage.Height);
        }
    }
}