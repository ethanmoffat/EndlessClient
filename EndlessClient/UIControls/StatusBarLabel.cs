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

        public StatusBarLabel(IClientWindowSizeProvider clientWindowSizeProvider,
                              IStatusLabelTextProvider statusLabelTextProvider)
            : base(Constants.FontSize07)
        {
            _statusLabelTextProvider = statusLabelTextProvider;
            DrawArea = new Rectangle(97, clientWindowSizeProvider.Height - 24, 1, 1);
        }

        protected override bool ShouldUpdate()
        {
            if (Text != _statusLabelTextProvider.StatusText)
            {
                Text = _statusLabelTextProvider.StatusText;
                Visible = true;
            }

            if ((DateTime.Now - _statusLabelTextProvider.SetTime).TotalMilliseconds > STATUS_LABEL_DISPLAY_TIME_MS)
                Visible = false;

            return base.ShouldUpdate();
        }
    }
}
