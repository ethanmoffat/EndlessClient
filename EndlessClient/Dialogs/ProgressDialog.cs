// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading;
using System.Threading.Tasks;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
	public class ProgressDialog : EODialogBase
	{
		private readonly TaskCompletionSource<XNADialogResult> _dialogResultCompletionSource;

		private TimeSpan? timeOpened;
		private readonly Texture2D _pbBackgroundTexture, _pbForegroundTexture;
		private int _pbWidth, _cancelRequests;

		public ProgressDialog(INativeGraphicsManager nativeGraphicsManager,
							  IGameStateProvider gameStateProvider,
							  IGraphicsDeviceProvider graphicsDeviceProvider,
							  string messageText,
							  string captionText)
			: base(nativeGraphicsManager)
		{
			bgTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 18);
			_setSize(bgTexture.Width, bgTexture.Height);

			message = new XNALabel(new Rectangle(18, 57, 1, 1), Constants.FontSize10)
			{
				ForeColor = ColorConstants.LightYellowText,
				Text = messageText,
				TextWidth = 254
			};
			message.SetParent(this);

			caption = new XNALabel(new Rectangle(59, 23, 1, 1), Constants.FontSize10)
			{
				ForeColor = ColorConstants.LightYellowText,
				Text = captionText
			};
			caption.SetParent(this);

			var cancel = new XNAButton(smallButtonSheet, new Vector2(181, 113),
				_getSmallButtonOut(SmallButton.Cancel),
				_getSmallButtonOver(SmallButton.Cancel));
			cancel.OnClick += DoCancel;
			cancel.SetParent(this);
			dlgButtons.Add(cancel);

			_pbBackgroundTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 19);

			_pbForegroundTexture = new Texture2D(Game.GraphicsDevice, 1, _pbBackgroundTexture.Height - 2); //foreground texture is just a fill
			var pbForeFill = new Color[_pbForegroundTexture.Width * _pbForegroundTexture.Height];
			for (int i = 0; i < pbForeFill.Length; ++i)
				pbForeFill[i] = Color.FromNonPremultiplied(0xb4, 0xdc, 0xe6, 0xff);
			_pbForegroundTexture.SetData(pbForeFill);

			_dialogResultCompletionSource = new TaskCompletionSource<XNADialogResult>();

			CenterAndFixDrawOrder(graphicsDeviceProvider, gameStateProvider);
		}

		public override void Update(GameTime gt)
		{
			if (timeOpened == null)
				timeOpened = gt.TotalGameTime;

			int pbPercent = (int)((gt.TotalGameTime.TotalSeconds - timeOpened.Value.TotalSeconds) / 10.0f * 100);
			_pbWidth = (int)Math.Round(pbPercent / 100.0f * _pbBackgroundTexture.Width);
			
			if (pbPercent >= 100)
				_dialogResultCompletionSource.SetResult(XNADialogResult.NO_BUTTON_PRESSED);

			base.Update(gt);
		}

		public override void Draw(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible)
				return;

			base.Draw(gt);

			SpriteBatch.Begin();
			SpriteBatch.Draw(_pbBackgroundTexture, new Vector2(15 + DrawAreaWithOffset.X, 95 + DrawAreaWithOffset.Y), Color.White);
			SpriteBatch.Draw(_pbForegroundTexture, new Rectangle(18 + DrawAreaWithOffset.X, 98 + DrawAreaWithOffset.Y, _pbWidth - 6, _pbForegroundTexture.Height - 4), Color.White);
			SpriteBatch.End();
		}

		public async Task WaitForCompletion()
		{
			var result = await _dialogResultCompletionSource.Task;
			Close(null, result);

			if (result == XNADialogResult.Cancel)
				throw new OperationCanceledException();
		}

		private void DoCancel(object sender, EventArgs e)
		{
			if (Interlocked.Increment(ref _cancelRequests) != 1)
				return;

			try
			{
				_dialogResultCompletionSource.SetResult(XNADialogResult.Cancel);
			}
			finally
			{
				Interlocked.Exchange(ref _cancelRequests, 0);
			}
		}
	}
}
