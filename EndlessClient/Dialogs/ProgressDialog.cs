using System;
using System.Threading;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EOLib.Config;
using EOLib.Graphics;
using EOLib.Shared;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class ProgressDialog : BaseEODialog
    {
        private readonly IConfigurationProvider _configurationProvider;

        private readonly IXNALabel _messageLabel, _captionLabel;
        private readonly IXNAButton _cancelButton;

        private TimeSpan? timeOpened;
        private readonly Texture2D _pbBackgroundTexture, _pbForegroundTexture;
        private int _pbWidth, _cancelRequests;

        public ProgressDialog(INativeGraphicsManager nativeGraphicsManager,
                              IGameStateProvider gameStateProvider,
                              IConfigurationProvider configurationProvider,
                              IEODialogButtonService eoDialogButtonService,
                              string messageText,
                              string captionText)
            : base(nativeGraphicsManager, gameStateProvider)
        {
            _configurationProvider = configurationProvider;

            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 18);

            _messageLabel = new XNALabel(Constants.FontSize10)
            {
                AutoSize = true,
                ForeColor = ColorConstants.LightYellowText,
                Text = messageText,
                TextWidth = 254,
                DrawPosition = new Vector2(18, 57)
            };
            _messageLabel.SetParentControl(this);

            _captionLabel = new XNALabel(Constants.FontSize10)
            {
                AutoSize = true,
                ForeColor = ColorConstants.LightYellowText,
                Text = captionText,
                DrawPosition = new Vector2(59, 23)
            };
            _captionLabel.SetParentControl(this);

            _cancelButton = new XNAButton(eoDialogButtonService.SmallButtonSheet,
                new Vector2(181, 113),
                eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel));
            _cancelButton.OnMouseDown += DoCancel;
            _cancelButton.SetParentControl(this);

            _pbBackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 19);

            _pbForegroundTexture = new Texture2D(Game.GraphicsDevice, 1, _pbBackgroundTexture.Height - 2); //foreground texture is just a fill
            var pbForeFill = new Color[_pbForegroundTexture.Width * _pbForegroundTexture.Height];
            for (int i = 0; i < pbForeFill.Length; ++i)
                pbForeFill[i] = Color.FromNonPremultiplied(0xb4, 0xdc, 0xe6, 0xff);
            _pbForegroundTexture.SetData(pbForeFill);

            CenterInGameView();
        }

        public override void Initialize()
        {
            _messageLabel.Initialize();
            _captionLabel.Initialize();
            _cancelButton.Initialize();

            base.Initialize();
        }

        protected override void OnUpdateControl(GameTime gt)
        {
            if (timeOpened == null)
                timeOpened = gt.TotalGameTime;

            var pbPercent = (int)((gt.TotalGameTime.TotalSeconds - timeOpened.Value.TotalSeconds) / _configurationProvider.AccountCreateTimeout.TotalSeconds * 100);
            _pbWidth = (int)Math.Round(pbPercent / 100.0f * _pbBackgroundTexture.Width);

            if (pbPercent >= 100)
                Close(XNADialogResult.NO_BUTTON_PRESSED);

            base.OnUpdateControl(gt);
        }

        protected override void OnDrawControl(GameTime gt)
        {
            base.OnDrawControl(gt);

            var pbBackgroundPosition = new Vector2(15 + DrawPositionWithParentOffset.X, 95 + DrawPositionWithParentOffset.Y);
            var pbForgroundArea = new Rectangle(18 + DrawAreaWithParentOffset.X, 98 + DrawAreaWithParentOffset.Y, _pbWidth - 6, _pbForegroundTexture.Height - 4);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_pbBackgroundTexture, pbBackgroundPosition, Color.White);
            _spriteBatch.Draw(_pbForegroundTexture, pbForgroundArea, Color.White);
            _spriteBatch.End();
        }

        private void DoCancel(object sender, EventArgs e)
        {
            if (Interlocked.Increment(ref _cancelRequests) != 1)
                return;

            try
            {
                Close(XNADialogResult.Cancel);
            }
            finally
            {
                Interlocked.Exchange(ref _cancelRequests, 0);
            }
        }
    }
}
