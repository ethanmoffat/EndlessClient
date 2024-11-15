using System;
using EndlessClient.Rendering;
using EOLib.Shared;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class TimeLabel : XNALabel
    {
        private DateTime _lastUpdateTime;

        public TimeLabel(IClientWindowSizeProvider windowSizeProvider)
            : base(Constants.FontSize07)
        {
            _lastUpdateTime = DateTime.Now;
            DrawArea = GetPositionBasedOnWindowSize(windowSizeProvider);
            windowSizeProvider.GameWindowSizeChanged += (_, _) => DrawArea = GetPositionBasedOnWindowSize(windowSizeProvider);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (DateTime.Now.Second != _lastUpdateTime.Second)
            {
                Text = $"{DateTime.Now.Hour,2:D2}:{DateTime.Now.Minute,2:D2}:{DateTime.Now.Second,2:D2}";

                _lastUpdateTime = DateTime.Now;
            }

            base.OnUpdateControl(gameTime);
        }

        private static Rectangle GetPositionBasedOnWindowSize(IClientWindowSizeProvider windowSizeProvider)
        {
            //original location: 558, 456
            var xLoc = windowSizeProvider.Width - 82;
            var yLoc = windowSizeProvider.Height - (windowSizeProvider.Resizable ? 15 : 26);

            return new Rectangle(xLoc, yLoc, 1, 1);
        }
    }
}
