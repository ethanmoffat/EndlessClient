using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.HUD.Chat;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;
using static System.Net.Mime.MediaTypeNames;

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
            NineWithScroll
        }

        public record InputInfo(string Label,
                                string DefaultValue = "",
                                int MaxChars = 12,
                                InputInfo.InputRestrict InputRestriction = InputInfo.InputRestrict.None)
        {
            public enum InputRestrict
            {
                None = 0,
                Uppercase = 1,
                Numeric = 2
            }
        }

        // can't use base class functionality because otherwise the bottom part of the dialog is drawn over the buttons
        private readonly Texture2D _background;
        private readonly Rectangle _backgroundSourceRectangle;

        private readonly Vector2 _bottomOverlayDrawPosition;
        private readonly Rectangle _bottomOverlaySource;
        private readonly Vector2 _rightOverlayDrawPosition;
        private readonly Rectangle _rightOverlaySource;
        private readonly ScrollBar _scrollBar;
        private readonly XNATextBox[] _inputBoxes;
        private XNALabel[] _inputLabels;
        private readonly DialogSize _dialogSize;
        private readonly INativeGraphicsManager _nativeGraphicsManager;

        private int _previousScrollOffset = -1;

        private bool _suppressTextChangedEvent;

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
            _nativeGraphicsManager = nativeGraphicsManager;
            _dialogSize = size;
            Vector2 okButtonPosition;
            Vector2 cancelButtonPosition;

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

                    _inputLabels = new XNALabel[2];
                    _inputBoxes = new XNATextBox[2];

                    okButtonPosition = new Vector2(73, 125);
                    cancelButtonPosition = new Vector2(166, 125);
                    break;

                case DialogSize.Three:
                    if (inputInfo.Length != 3)
                    {
                        throw new ArgumentException("Not enough input labels were provided");
                    }

                    _backgroundSourceRectangle = new Rectangle(0, 0, 330, 194);

                    SetSize(330, 194);
                    _bottomOverlayDrawPosition = new Vector2(0, 134);
                    _bottomOverlaySource = new Rectangle(0, 240, 330, 59);

                    _inputLabels = new XNALabel[3];
                    _inputBoxes = new XNATextBox[3];

                    okButtonPosition = new Vector2(73, 148);
                    cancelButtonPosition = new Vector2(166, 148);
                    break;

                case DialogSize.NineWithScroll:
                    if (inputInfo.Length != 9)
                    {
                        throw new ArgumentException("Not enough input labels were provided");
                    }

                    _backgroundSourceRectangle = new Rectangle(0, 0, 330, 210);

                    SetSize(330, 210);
                    _bottomOverlayDrawPosition = new Vector2(0, 190);
                    _rightOverlayDrawPosition = new Vector2(270, 0);
                    _bottomOverlaySource = new Rectangle(0, 240, 330, 59);
                    _rightOverlaySource = new Rectangle(330, 0, 40, 190);
                    _scrollBar = new ScrollBar(new Vector2(279, 72), new Vector2(16, 107), ScrollBarColors.LightOnDark, _nativeGraphicsManager)
                    {
                        LinesToRender = 5,
                        Visible = true
                    };
                    _scrollBar.SetParentControl(this);
                    _scrollBar.UpdateDimensions(9);
                    SetScrollWheelHandler(_scrollBar);

                    _inputLabels = new XNALabel[9];
                    _inputBoxes = new XNATextBox[9];

                    okButtonPosition = new Vector2(73, 195);
                    cancelButtonPosition = new Vector2(166, 195);

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

            int yCoord = 69; // nice
            for (int i = 0; i < inputInfo.Length; i++)
            {
                _inputLabels[i] = new XNALabel(Constants.FontSize10)
                {
                    AutoSize = true,
                    ForeColor = ColorConstants.LightGrayDialogMessage,
                    Text = inputInfo[i].Label,
                    DrawPosition = new Vector2(24, yCoord),
                };
                _inputLabels[i].SetParentControl(this);

                _inputBoxes[i] = new XNATextBox(new Rectangle(126, yCoord, 168, 19), Constants.FontSize08, caretTexture: contentProvider.Textures[ContentProvider.Cursor])
                {
                    MaxChars = inputInfo[i].MaxChars,
                    LeftPadding = 4,
                    TextColor = ColorConstants.LightBeigeText,
                    MaxWidth = 160,
                    TabOrder = i
                };
                _inputBoxes[i].SetParentControl(this);
                // Initialize must be called here because it sets up the label for text rendering; otherwise, the call to set the default value fails
                _inputBoxes[i].Initialize();
                _inputBoxes[i].Text = inputInfo[i].DefaultValue;

                if (inputInfo[i].InputRestriction != InputInfo.InputRestrict.None)
                {
                    var inputRestriction = inputInfo[i].InputRestriction;
                    _inputBoxes[i].OnTextChanged += (sender, _) =>
                    {
                        if (_suppressTextChangedEvent) return;

                        if (sender is XNATextBox inputBox)
                        {
                            _suppressTextChangedEvent = true;
                            switch (inputRestriction)
                            {
                                case InputInfo.InputRestrict.Uppercase:
                                    inputBox.Text = inputBox.Text.ToUpper();
                                    break;
                                case InputInfo.InputRestrict.Numeric:
                                    if (inputBox.Text.Length > 0)
                                    {
                                        if (!char.IsNumber(inputBox.Text[inputBox.Text.Length - 1]))
                                        {
                                            inputBox.Text = inputBox.Text.Remove(inputBox.Text.Length - 1);
                                        }
                                    }
                                    break;
                            }
                            _suppressTextChangedEvent = false;
                        }
                    };
                }

                yCoord += 23;
            }

            var ok = new XNAButton(eoDialogButtonService.SmallButtonSheet,
                okButtonPosition,
                eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok),
                eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok));
            ok.OnClick += (_, _) => Close(XNADialogResult.OK);
            ok.SetParentControl(this);

            var cancel = new XNAButton(eoDialogButtonService.SmallButtonSheet,
                cancelButtonPosition,
                eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel));
            cancel.OnClick += (_, _) => Close(XNADialogResult.Cancel);
            cancel.SetParentControl(this);

            DialogClosed += (_, _) => chatTextBoxActions.FocusChatTextBox();

            CenterInGameView();
        }

        public override void Initialize()
        {
            foreach (var label in _inputLabels)
                label.Initialize();

            _inputBoxes[0].Selected = true;

            base.Initialize();
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            // need to redo drawing background image so controls get drawn on top of background
            _spriteBatch.Begin();
            _spriteBatch.Draw(_background, DrawAreaWithParentOffset, _backgroundSourceRectangle, Color.White);
            _spriteBatch.Draw(_background, DrawPositionWithParentOffset + _bottomOverlayDrawPosition, _bottomOverlaySource, Color.White);
            _spriteBatch.Draw(_background, DrawPositionWithParentOffset + _rightOverlayDrawPosition, _rightOverlaySource, Color.White);
            _spriteBatch.End();
            base.OnDrawControl(gameTime);
        }

        protected override void OnUpdateControl(GameTime gameTime)
        {
            if (_dialogSize != DialogSize.NineWithScroll)
            {
                base.OnUpdateControl(gameTime);
                return;
            }

            if (_scrollBar.ScrollOffset != _previousScrollOffset)
            {
                for (int i = 0; i < _inputBoxes.Length; i++)
                {
                    var currBox = _inputBoxes[i];
                    var currLabel = _inputLabels[i];

                    if (i < _scrollBar.ScrollOffset)
                    {
                        currBox.Visible = false;
                        currLabel.Visible = false;
                        continue;
                    }

                    if (i < _scrollBar.LinesToRender + _scrollBar.ScrollOffset)
                    {
                        currBox.Visible = true;
                        currLabel.Visible = true;

                        int relativeIndex = i - _scrollBar.ScrollOffset;
                        currBox.DrawPosition = new Vector2(currBox.DrawPosition.X, 69 + relativeIndex * 23);
                        currLabel.DrawPosition = new Vector2(currLabel.DrawPosition.X, 69 + relativeIndex * 23);
                    }

                    else
                    {
                        currBox.Visible = false;
                        currLabel.Visible = false;
                    }
                }

                _previousScrollOffset = _scrollBar.ScrollOffset;
            }
            base.OnUpdateControl(gameTime);
        }

        public override void CenterInGameView()
        {
            var viewport = Game.GraphicsDevice.Viewport;
            var area = _backgroundSourceRectangle;
            DrawPosition = new Vector2((viewport.Width - area.Width) / 2, (viewport.Height - area.Height) / 2);

            if (!Game.Window.AllowUserResizing)
                DrawPosition = new Vector2(DrawPosition.X, (330 - DrawArea.Height) / 2f);
        }
    }
}
