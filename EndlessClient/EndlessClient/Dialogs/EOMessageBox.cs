// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.Graphics;
using EOLib.IO;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.Dialogs
{
	public enum EOMessageBoxStyle
	{
		SmallDialogLargeHeader,
		SmallDialogSmallHeader,
		LargeDialogSmallHeader
	}

	public class EOMessageBox : EODialogBase
	{
		public EOMessageBox(string msgText, string captionText = "", XNADialogButtons whichButtons = XNADialogButtons.Ok, EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogLargeHeader)
		{
			this.whichButtons = whichButtons;

			var useSmallHeader = true;
			switch (style)
			{
				case EOMessageBoxStyle.SmallDialogLargeHeader:
					bgTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 18);
					useSmallHeader = false;
					break;
				case EOMessageBoxStyle.SmallDialogSmallHeader:
					bgTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 23);
					break;
				case EOMessageBoxStyle.LargeDialogSmallHeader:
					bgTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PreLoginUI, 25);
					break;
				default:
					throw new ArgumentOutOfRangeException("style", "Unrecognized dialog style!");
			}
			_setSize(bgTexture.Width, bgTexture.Height);

			message = new XNALabel(new Rectangle(18, 57, 1, 1), Constants.FontSize10);
			if (useSmallHeader)
			{
				//179, 119
				//caption 197, 128
				//message 197, 156
				//ok: 270, 201
				//cancel: 363, 201
				message.DrawLocation = new Vector2(18, 40);
			}
			message.ForeColor = Constants.LightYellowText;
			message.Text = msgText;
			message.TextWidth = 254;
			message.SetParent(this);

			caption = new XNALabel(new Rectangle(59, 23, 1, 1), Constants.FontSize10);
			if (useSmallHeader)
			{
				caption.DrawLocation = new Vector2(18, 12);
			}
			caption.ForeColor = Constants.LightYellowText;
			caption.Text = captionText;
			caption.SetParent(this);

			XNAButton ok, cancel;
			switch (whichButtons)
			{
				case XNADialogButtons.Ok:
					ok = new XNAButton(smallButtonSheet, new Vector2(181, 113), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
					ok.OnClick += (sender, e) => Close(ok, XNADialogResult.OK);
					ok.SetParent(this);
					dlgButtons.Add(ok);
					break;
				case XNADialogButtons.Cancel:
					cancel = new XNAButton(smallButtonSheet, new Vector2(181, 113), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
					cancel.OnClick += (sender, e) => Close(cancel, XNADialogResult.Cancel);
					cancel.SetParent(this);
					dlgButtons.Add(cancel);
					break;
				case XNADialogButtons.OkCancel:
					//implement this more fully when it is needed
					//update draw location of ok button to be on left?
					ok = new XNAButton(smallButtonSheet, new Vector2(89, 113), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
					ok.OnClick += (sender, e) => Close(ok, XNADialogResult.OK);
					ok.SetParent(this);

					cancel = new XNAButton(smallButtonSheet, new Vector2(181, 113), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
					cancel.OnClick += (s, e) => Close(cancel, XNADialogResult.Cancel);
					cancel.SetParent(this);

					dlgButtons.Add(ok);
					dlgButtons.Add(cancel);
					break;
			}

			if (useSmallHeader)
			{
				if (style == EOMessageBoxStyle.SmallDialogSmallHeader)
					foreach (XNAButton btn in dlgButtons)
						btn.DrawLocation = new Vector2(btn.DrawLocation.X, 82);
				else
					foreach (XNAButton btn in dlgButtons)
						btn.DrawLocation = new Vector2(btn.DrawLocation.X, 148);
			}

			endConstructor();
		}

		public static void Show(string message, string caption = "", XNADialogButtons buttons = XNADialogButtons.Ok, EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogLargeHeader, OnDialogClose closingEvent = null)
		{
			EOMessageBox dlg = new EOMessageBox(message, caption, buttons, style);
			if (closingEvent != null)
				dlg.DialogClosing += closingEvent;
		}

		public static void Show(DATCONST1 resource, XNADialogButtons whichButtons = XNADialogButtons.Ok,
			EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogLargeHeader, OnDialogClose closingEvent = null)
		{
			if (!World.Initialized)
				throw new WorldLoadException("Unable to create dialog! World must be loaded and initialized.");

			EDFFile file = World.Instance.DataFiles[World.Instance.Localized1];

			Show(file.Data[(int)resource + 1], file.Data[(int)resource], whichButtons, style, closingEvent);
		}

		public static void Show(string prependData, DATCONST1 resource, XNADialogButtons whichButtons = XNADialogButtons.Ok,
			EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogLargeHeader, OnDialogClose closingEvent = null)
		{
			if (!World.Initialized)
				throw new WorldLoadException("Unable to create dialog! World must be loaded and initialized.");

			EDFFile file = World.Instance.DataFiles[World.Instance.Localized1];

			string message = prependData + file.Data[(int)resource + 1];
			Show(message, file.Data[(int)resource], whichButtons, style, closingEvent);
		}

		public static void Show(DATCONST1 resource, string extraData, XNADialogButtons whichButtons = XNADialogButtons.Ok,
			EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogLargeHeader, OnDialogClose closingEvent = null)
		{
			if (!World.Initialized)
				throw new WorldLoadException("Unable to create dialog! World must be loaded and initialized.");

			EDFFile file = World.Instance.DataFiles[World.Instance.Localized1];

			string message = file.Data[(int)resource + 1] + extraData;
			Show(message, file.Data[(int)resource], whichButtons, style, closingEvent);
		}
	}
}
