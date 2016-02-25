// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EndlessClient.Controls;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.Dialogs
{
	public class ScrollingMessageDialog : EODialogBase
	{
		private readonly ScrollBar scrollBar;
		private readonly List<string> chatStrings = new List<string>();
		private readonly TextSplitter textSplitter;

		public new string MessageText
		{
			set
			{
				chatStrings.Clear();
				textSplitter.Text = value;

				//special case: blank line, like in the news panel between news items
				if (string.IsNullOrWhiteSpace(value))
				{
					chatStrings.Add(" ");
					scrollBar.UpdateDimensions(chatStrings.Count);
					return;
				}

				//don't do multi-line processing if we don't need to
				if (!textSplitter.NeedsProcessing)
				{
					chatStrings.Add(value);
					scrollBar.UpdateDimensions(chatStrings.Count);
					return;
				}

				chatStrings.AddRange(textSplitter.SplitIntoLines());

				scrollBar.UpdateDimensions(chatStrings.Count);
				scrollBar.LinesToRender = (int)Math.Round(110.0f / 13); //draw area for the text is 117px, 13px per line
				if (scrollBar.LinesToRender < chatStrings.Count)
					scrollBar.SetDownArrowFlashSpeed(500);
			}
		}

		public ScrollingMessageDialog(string msgText)
		{
			textSplitter = new TextSplitter("", EOGame.Instance.DBGFont) { LineLength = 275 };

			bgTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 40);
			_setSize(bgTexture.Width, bgTexture.Height);

			XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(138, 197), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
			ok.OnClick += (sender, e) => Close(ok, XNADialogResult.OK);
			ok.SetParent(this);
			dlgButtons.Add(ok);

			scrollBar = new ScrollBar(this, new Vector2(320, 66), new Vector2(16, 119), ScrollBarColors.LightOnMed);
			MessageText = msgText;

			endConstructor();
		}

		public override void Draw(GameTime gt)
		{
			if ((parent != null && !parent.Visible) || !Visible)
				return;

			base.Draw(gt);
			if (scrollBar == null) return; //prevent nullreferenceexceptions

			SpriteBatch.Begin();
			Vector2 pos = new Vector2(27 + (int)DrawLocation.X, 69 + (int)DrawLocation.Y);

			for (int i = scrollBar.ScrollOffset; i < scrollBar.ScrollOffset + scrollBar.LinesToRender; ++i)
			{
				if (i >= chatStrings.Count)
					break;

				string strToDraw = chatStrings[i];

				SpriteBatch.DrawString(EOGame.Instance.DBGFont, strToDraw, new Vector2(pos.X, pos.Y + (i - scrollBar.ScrollOffset) * 13), Constants.LightGrayText);
			}

			SpriteBatch.End();
		}
	}
}
