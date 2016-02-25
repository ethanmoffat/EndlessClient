// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
	public class ProgressDialog : EODialogBase
	{
		private TimeSpan? timeOpened;
		private readonly Texture2D pbBackText, pbForeText;
		private int pbWidth;

		public ProgressDialog(string msgText, string captionText = "")
		{
			bgTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 18);
			_setSize(bgTexture.Width, bgTexture.Height);

			message = new XNALabel(new Rectangle(18, 57, 1, 1), Constants.FontSize10)
			{
				ForeColor = Constants.LightYellowText,
				Text = msgText,
				TextWidth = 254
			};
			message.SetParent(this);

			caption = new XNALabel(new Rectangle(59, 23, 1, 1), Constants.FontSize10)
			{
				ForeColor = Constants.LightYellowText,
				Text = captionText
			};
			caption.SetParent(this);

			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(181, 113), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
			ok.OnClick += (sender, e) => Close(ok, XNADialogResult.Cancel);
			ok.SetParent(this);
			dlgButtons.Add(ok);

			pbBackText = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 19);

			pbForeText = new Texture2D(Game.GraphicsDevice, 1, pbBackText.Height - 2); //foreground texture is just a fill
			Color[] pbForeFill = new Color[pbForeText.Width * pbForeText.Height];
			for (int i = 0; i < pbForeFill.Length; ++i)
				pbForeFill[i] = Color.FromNonPremultiplied(0xb4, 0xdc, 0xe6, 0xff);
			pbForeText.SetData(pbForeFill);

			endConstructor();
		}

		public override void Update(GameTime gt)
		{
			if (timeOpened == null) //set timeOpened on first call to Update
				timeOpened = gt.TotalGameTime;

			int pbPercent = (int)((gt.TotalGameTime.TotalSeconds - timeOpened.Value.TotalSeconds) / 10.0f * 100);

			pbWidth = (int)Math.Round(pbPercent / 100.0f * pbBackText.Width);
			if (pbPercent >= 100)
				Close(null, XNADialogResult.NO_BUTTON_PRESSED);

			base.Update(gt);
		}

		public override void Draw(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible)
				return;

			base.Draw(gt);

			SpriteBatch.Begin();
			SpriteBatch.Draw(pbBackText, new Vector2(15 + DrawAreaWithOffset.X, 95 + DrawAreaWithOffset.Y), Color.White);
			SpriteBatch.Draw(pbForeText, new Rectangle(18 + DrawAreaWithOffset.X, 98 + DrawAreaWithOffset.Y, pbWidth - 6, pbForeText.Height - 4), Color.White);
			SpriteBatch.End();
		}
	}
}
