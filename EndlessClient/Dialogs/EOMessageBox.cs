using System;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EOLib;
using EOLib.Graphics;
using EOLib.Localization;
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

    public class EOMessageBox : BaseEODialog
    {
        private readonly IXNALabel _messageLabel, _captionLabel;
        private readonly IXNAButton _ok, _cancel;

        public EOMessageBox(INativeGraphicsManager graphicsManager,
                            IGameStateProvider gameStateProvider,
                            IEODialogButtonService eoDialogButtonService,
                            string message,
                            string caption = "",
                            EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogSmallHeader,
                            EODialogButtons whichButtons = EODialogButtons.Ok)
            : base(graphicsManager, gameStateProvider)
        {
            var useSmallHeader = style != EOMessageBoxStyle.SmallDialogLargeHeader;

            int graphic;
            switch (style)
            {
                case EOMessageBoxStyle.SmallDialogLargeHeader: graphic = 18; break;
                case EOMessageBoxStyle.SmallDialogSmallHeader: graphic = 23; break;
                case EOMessageBoxStyle.LargeDialogSmallHeader: graphic = 25; break;
                default: throw new ArgumentOutOfRangeException(nameof(style), "Unrecognized dialog style!");
            }

            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, graphic);

            _messageLabel = new XNALabel(Constants.FontSize10)
            {
                AutoSize = true,
                ForeColor = ColorConstants.LightYellowText,
                Text = message,
                TextWidth = 240,
                DrawPosition = new Vector2(21, useSmallHeader ? 40 : 62),
                WrapBehavior = WrapBehavior.WrapToNewLine,
            };

            _messageLabel.SetParentControl(this);

            _captionLabel = new XNALabel(Constants.FontSize10)
            {
                AutoSize = true,
                ForeColor = ColorConstants.LightYellowText,
                Text = caption,
                TextWidth = 254,
                DrawPosition = useSmallHeader ? new Vector2(18, 12) : new Vector2(60, 27)
            };
            _captionLabel.SetParentControl(this);

            var smallButtonSheet = eoDialogButtonService.SmallButtonSheet;
            var okOut = eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok);
            var okOver = eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok);
            var cancelOut = eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel);
            var cancelOver = eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel);

            switch (whichButtons)
            {
                case EODialogButtons.Ok:
                    _ok = new XNAButton(smallButtonSheet, new Vector2(181, 113), okOut, okOver);
                    _ok.OnClick += (sender, e) => Close(XNADialogResult.OK);
                    _ok.SetParentControl(this);
                    break;
                case EODialogButtons.Cancel:
                    _cancel = new XNAButton(smallButtonSheet, new Vector2(181, 113), cancelOut, cancelOver);
                    _cancel.OnClick += (sender, e) => Close(XNADialogResult.Cancel);
                    _cancel.SetParentControl(this);
                    break;
                case EODialogButtons.OkCancel:
                    //implement this more fully when it is needed
                    //update draw location of ok button to be on left?
                    _ok = new XNAButton(smallButtonSheet, new Vector2(89, 113), okOut, okOver);
                    _ok.OnClick += (sender, e) => Close(XNADialogResult.OK);
                    _ok.SetParentControl(this);

                    _cancel = new XNAButton(smallButtonSheet, new Vector2(181, 113), cancelOut, cancelOver);
                    _cancel.OnClick += (s, e) => Close(XNADialogResult.Cancel);
                    _cancel.SetParentControl(this);
                    break;
            }

            if (useSmallHeader)
            {
                if (_ok != null)
                {
                    _ok.DrawPosition = new Vector2(_ok.DrawPosition.X,
                        style == EOMessageBoxStyle.SmallDialogSmallHeader ? 82 : 148);
                }

                if (_cancel != null)
                {
                    _cancel.DrawPosition = new Vector2(_cancel.DrawPosition.X,
                        style == EOMessageBoxStyle.SmallDialogSmallHeader ? 82 : 148);
                }
            }

            CenterInGameView();
        }

        public override void Initialize()
        {
            _messageLabel.Initialize();
            _captionLabel.Initialize();

            if (_ok != null)
                _ok.Initialize();
            if (_cancel != null)
                _cancel.Initialize();

            base.Initialize();
        }

        #region Deprecated

        public static void Show(string message, string caption = "", EODialogButtons buttons = EODialogButtons.Ok,
            EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogLargeHeader, XNAControls.Old.XNADialog.OnDialogClose closingEvent = null)
        {
            throw new NotImplementedException("Static message box display is deprecated and will be removed in the future");
        }

        public static void Show(DialogResourceID resource, EODialogButtons whichButtons = EODialogButtons.Ok,
            EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogLargeHeader, XNAControls.Old.XNADialog.OnDialogClose closingEvent = null)
        {
            throw new NotImplementedException("Static message box display is deprecated and will be removed in the future");
        }

        public static void Show(string prependData, DialogResourceID resource, EODialogButtons whichButtons = EODialogButtons.Ok,
            EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogLargeHeader, XNAControls.Old.XNADialog.OnDialogClose closingEvent = null)
        {
            throw new NotImplementedException("Static message box display is deprecated and will be removed in the future");
        }

        public static void Show(DialogResourceID resource, string extraData, EODialogButtons whichButtons = EODialogButtons.Ok,
            EOMessageBoxStyle style = EOMessageBoxStyle.SmallDialogLargeHeader, XNAControls.Old.XNADialog.OnDialogClose closingEvent = null)
        {
            throw new NotImplementedException("Static message box display is deprecated and will be removed in the future");
        }

        #endregion
    }
}
