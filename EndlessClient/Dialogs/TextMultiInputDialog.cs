using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD.Chat;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class TextMultiInputDialog : BaseEODialog
    {
        public enum DialogSize
        {
            Two,
            Three,
            Four,
            Five,
            FiveWithScroll,
        }

        public struct InputInfo
        {
            public string Label;
            public int MaxChars;
        }

        // can't use base class functionality because otherwise the bottom part of the dialog is drawn over the buttons
        private readonly Texture2D _background;
        private readonly Rectangle _backgroundSourceRectangle;

        private readonly Vector2 _bottomOverlayDrawPosition;
        private readonly Rectangle _bottomOverlaySource;

        private readonly IXNATextBox[] _inputBoxes;

        public IReadOnlyList<string> Responses => _inputBoxes.Select(x => x.Text).ToList();

        public TextMultiInputDialog(INativeGraphicsManager nativeGraphicsManager,
                                    IChatTextBoxActions chatTextBoxActions,
                                    IEODialogButtonService eoDialogButtonService,
                                    IContentProvider contentProvider,
                                    DialogSize size,
                                    string title,
                                    string prompt,
                                    InputInfo[] inputInfo)
            : base(nativeGraphicsManager, isInGame: true)
        {
            _background = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 57);

            switch (size)
            {
                case DialogSize.Two:
                    if (inputInfo.Length != 2)
                    {
                        throw new ArgumentException("Not enough input labels were provided");
                    }

                    _backgroundSourceRectangle = new Rectangle(0, 0, 330, 170);

                    SetSize(330, 170);
                    _bottomOverlayDrawPosition = new Vector2(0, 111);
                    _bottomOverlaySource = new Rectangle(0, 240, 330, 59);

                    _inputBoxes = new IXNATextBox[2];
                    break;
                default: throw new NotImplementedException();
            }

            var lblTitle = new XNALabel(Constants.FontSize10)
            {
                AutoSize = true,
                ForeColor = ColorConstants.LightYellowText,
                Text = title,
                TextWidth = 254,
                DrawPosition = new Vector2(18, 12),
            };
            lblTitle.Initialize();
            lblTitle.SetParentControl(this);

            var lblPrompt = new XNALabel(Constants.FontSize10)
            {
                AutoSize = true,
                ForeColor = ColorConstants.LightGrayDialogMessage,
                Text = prompt,
                TextWidth = 254,
                DrawPosition = new Vector2(19, 40),
                WrapBehavior = WrapBehavior.WrapToNewLine,
            };
            lblPrompt.Initialize();
            lblPrompt.SetParentControl(this);

            int yCoord = 69;
            for (int i = 0; i < inputInfo.Length; i++)
            {
                var nextLabel = new XNALabel(Constants.FontSize10)
                {
                    AutoSize = true,
                    ForeColor = ColorConstants.LightGrayDialogMessage,
                    Text = inputInfo[i].Label,
                    DrawPosition = new Vector2(24, yCoord),
                };
                nextLabel.Initialize();
                nextLabel.SetParentControl(this);

                _inputBoxes[i] = new XNATextBox(new Rectangle(126, yCoord, 168, 19), Constants.FontSize08, caretTexture: contentProvider.Textures[ContentProvider.Cursor])
                {
                    MaxChars = inputInfo[i].MaxChars,
                    LeftPadding = 4,
                    TextColor = ColorConstants.LightBeigeText,
                    MaxWidth = 160,
                };
                _inputBoxes[i].SetParentControl(this);

                yCoord += 24;
            }

            var ok = new XNAButton(eoDialogButtonService.SmallButtonSheet,
                new Vector2(73, 125),
                eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok),
                eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok));
            ok.OnClick += (_, _) => Close(XNADialogResult.OK);
            ok.SetParentControl(this);

            var cancel = new XNAButton(eoDialogButtonService.SmallButtonSheet,
                new Vector2(166, 125),
                eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel));
            cancel.OnClick += (_, _) => Close(XNADialogResult.Cancel);
            cancel.SetParentControl(this);

            DialogClosed += (_, _) => chatTextBoxActions.FocusChatTextBox();

            CenterInGameView();

            DrawPosition += new Vector2(-160, 0);
        }

        public override void Initialize()
        {
            foreach (var box in _inputBoxes)
                box.Initialize();

            _inputBoxes[0].Selected = true;

            base.Initialize();
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            // need to redo drawing background image so controls get drawn on top of background
            _spriteBatch.Begin();
            _spriteBatch.Draw(_background, DrawAreaWithParentOffset, _backgroundSourceRectangle, Color.White);
            _spriteBatch.Draw(_background, DrawPositionWithParentOffset + _bottomOverlayDrawPosition, _bottomOverlaySource, Color.White);
            _spriteBatch.End();

            base.OnDrawControl(gameTime);
        }
    }
}
