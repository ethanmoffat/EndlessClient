// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.HUD;
using EndlessClient.Rendering;
using EOLib;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.UIControls
{
    public class StatusBarLabel : XNALabel
    {
        private const int STATUS_LABEL_DISPLAY_TIME_MS = 3000;

        private readonly IStatusLabelTextProvider _statusLabelTextProvider;

        private readonly bool _constructed;

        public StatusBarLabel(IClientWindowSizeProvider clientWindowSizeProvider,
                              IStatusLabelTextProvider statusLabelTextProvider)
            : base(GetPositionBasedOnWindowSize(clientWindowSizeProvider), Constants.FontSize07)
        {
            _statusLabelTextProvider = statusLabelTextProvider;
            _constructed = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (!_constructed)
                return;

            if (Text != _statusLabelTextProvider.StatusText)
            {
                Text = _statusLabelTextProvider.StatusText;
                Visible = true;
            }

            if ((DateTime.Now - _statusLabelTextProvider.SetTime).TotalMilliseconds > STATUS_LABEL_DISPLAY_TIME_MS)
                Visible = false;

            base.Update(gameTime);
        }

        private static Rectangle GetPositionBasedOnWindowSize(IClientWindowSizeProvider clientWindowSizeProvider)
        {
            return new Rectangle(97, clientWindowSizeProvider.Height - 24, 1, 1);
        }
    }
}
