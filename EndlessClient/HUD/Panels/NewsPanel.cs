// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering;
using EndlessClient.UIControls;
using EOLib.Domain.Login;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.HUD.Panels
{
    public class NewsPanel : XNAPanel, IHudPanel
    {
        private readonly IChatRenderer _chatRenderer;
        private readonly INewsProvider _newsProvider;

        private readonly ScrollBar _scrollBar;

        public NewsPanel(INativeGraphicsManager nativeGraphicsManager,
                         IChatRenderer chatRenderer,
                         INewsProvider newsProvider)
            : base(new Rectangle(102, 330, 1, 1))
        {
            _chatRenderer = chatRenderer;
            _newsProvider = newsProvider;

            BackgroundImage = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 48);
            _setSize(BackgroundImage.Width, BackgroundImage.Height);

            //abs coordiantes: 568 331
            _scrollBar = new ScrollBar(this, new Vector2(467, 20), new Vector2(16, 97), ScrollBarColors.LightOnMed)
            {
                LinesToRender = 7,
                Visible = true
            };

            if (_newsProvider.NewsText.Count > _scrollBar.LinesToRender)
                _scrollBar.SetDownArrowFlashSpeed(500);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            _chatRenderer.RenderNews(_newsProvider.NewsText, _scrollBar.ScrollOffset, _scrollBar.LinesToRender);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _chatRenderer.Dispose();

            base.Dispose(disposing);
        }
    }
}
