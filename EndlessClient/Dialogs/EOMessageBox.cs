// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Graphics;
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
		private readonly TaskCompletionSource<XNADialogResult> _dialogClosedTask;

		public EOMessageBox(INativeGraphicsManager graphicsManager,
							IGameStateProvider gameStateProvider,
							IGraphicsDeviceProvider graphicsDeviceProvider,
							string message,
							string caption = "",
							EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader,
							XNADialogButtons whichButtons = XNADialogButtons.Ok)
			: base(graphicsManager)
		{
			this.whichButtons = whichButtons;

			var useSmallHeader = true;
			switch (style)
			{
				case EOMessageBoxStyle.SmallDialogLargeHeader:
					bgTexture = graphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 18);
					useSmallHeader = false;
					break;
				case EOMessageBoxStyle.SmallDialogSmallHeader:
					bgTexture = graphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 23);
					break;
				case EOMessageBoxStyle.LargeDialogSmallHeader:
					bgTexture = graphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 25);
					break;
				default:
					throw new ArgumentOutOfRangeException("style", "Unrecognized dialog style!");
			}
			_setSize(bgTexture.Width, bgTexture.Height);

			this.message = new XNALabel(new Rectangle(18, 57, 1, 1), Constants.FontSize10);
			if (useSmallHeader)
			{
				//179, 119
				//caption 197, 128
				//message 197, 156
				//ok: 270, 201
				//cancel: 363, 201
				this.message.DrawLocation = new Vector2(18, 40);
			}
			this.message.ForeColor = Constants.LightYellowText;
			this.message.Text = message;
			this.message.TextWidth = 254;
			this.message.SetParent(this);

			this.caption = new XNALabel(new Rectangle(59, 23, 1, 1), Constants.FontSize10);
			if (useSmallHeader)
			{
				this.caption.DrawLocation = new Vector2(18, 12);
			}
			this.caption.ForeColor = Constants.LightYellowText;
			this.caption.Text = caption;
			this.caption.SetParent(this);

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

			_dialogClosedTask = new TaskCompletionSource<XNADialogResult>();
			DialogClosing += DialogClosingHandler;

			CenterAndFixDrawOrder(graphicsDeviceProvider, gameStateProvider);
		}

		private void DialogClosingHandler(object sender, CloseDialogEventArgs e)
		{
			_dialogClosedTask.SetResult(e.Result);
		}

		public new async Task<XNADialogResult> Show()
		{
			return await _dialogClosedTask.Task;
		}

		public static void Show(string message, string caption = "", XNADialogButtons buttons = XNADialogButtons.Ok, EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogLargeHeader, OnDialogClose closingEvent = null)
		{
			throw new NotImplementedException("Static message box display is deprecated and will be removed in the future");
		}

		public static void Show(DATCONST1 resource, XNADialogButtons whichButtons = XNADialogButtons.Ok,
			EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogLargeHeader, OnDialogClose closingEvent = null)
		{
			if (!OldWorld.Initialized)
				throw new WorldLoadException("Unable to create dialog! World must be loaded and initialized.");

			var file = OldWorld.Instance.DataFiles[OldWorld.Instance.Localized1];
			Show(file.Data[(int)resource + 1], file.Data[(int)resource], whichButtons, style, closingEvent);
		}

		public static void Show(string prependData, DATCONST1 resource, XNADialogButtons whichButtons = XNADialogButtons.Ok,
			EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogLargeHeader, OnDialogClose closingEvent = null)
		{
			if (!OldWorld.Initialized)
				throw new WorldLoadException("Unable to create dialog! World must be loaded and initialized.");

			var file = OldWorld.Instance.DataFiles[OldWorld.Instance.Localized1];
			var message = prependData + file.Data[(int)resource + 1];
			Show(message, file.Data[(int)resource], whichButtons, style, closingEvent);
		}

		public static void Show(DATCONST1 resource, string extraData, XNADialogButtons whichButtons = XNADialogButtons.Ok,
			EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogLargeHeader, OnDialogClose closingEvent = null)
		{
			if (!OldWorld.Initialized)
				throw new WorldLoadException("Unable to create dialog! World must be loaded and initialized.");

			var file = OldWorld.Instance.DataFiles[OldWorld.Instance.Localized1];
			var message = file.Data[(int)resource + 1] + extraData;
			Show(message, file.Data[(int)resource], whichButtons, style, closingEvent);
		}
	}
}
