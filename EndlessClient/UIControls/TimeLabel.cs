// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Rendering;
using EOLib;
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
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (DateTime.Now.Second != _lastUpdateTime.Second)
            {
                Text = string.Format("{0,2:D2}:{1,2:D2}:{2,2:D2}",
                    DateTime.Now.Hour,
                    DateTime.Now.Minute,
                    DateTime.Now.Second);

                _lastUpdateTime = DateTime.Now;
            }

            base.OnUpdateControl(gameTime);
        }

        private static Rectangle GetPositionBasedOnWindowSize(IClientWindowSizeProvider windowSizeProvider)
        {
            //original location: 558, 456
            var xLoc = windowSizeProvider.Width - 82;
            var yLoc = windowSizeProvider.Height - 24;

            return new Rectangle(xLoc, yLoc, 1, 1);
        }
    }
}
