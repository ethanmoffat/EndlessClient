// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Factories;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class CreateCharacterDialog : BaseEODialog
    {
        private readonly IEOMessageBoxFactory _messageBoxFactory;

        private readonly IXNATextBox _inputBox;
        private readonly IXNAButton[] _arrowButtons = new IXNAButton[4];

        private readonly IXNAButton _ok, _cancel;

        private readonly CreateCharacterControl _characterControl;

        private readonly Rectangle[] _srcRectangles = new Rectangle[4];
        private readonly Texture2D _charCreateSheet;

        public string Name => _inputBox.Text.Trim();

        private ICharacterRenderProperties RenderProperties => _characterControl.RenderProperties;
        public byte Gender => RenderProperties.Gender;
        public byte HairStyle => RenderProperties.HairStyle;
        public byte HairColor => RenderProperties.HairColor;
        public byte Race => RenderProperties.Race;

        public CreateCharacterDialog(
            INativeGraphicsManager nativeGraphicsManager,
            IGameStateProvider gameStateProvider,
            ICharacterRendererFactory rendererFactory,
            ContentManager contentManager,
            KeyboardDispatcher dispatcher,
            IEOMessageBoxFactory messageBoxFactory,
            IEODialogButtonService eoDialogButtonService)
            : base(gameStateProvider)
        {
            _messageBoxFactory = messageBoxFactory;
            BackgroundTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 20);

            _charCreateSheet = nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 22);

            var cursorTexture = contentManager.Load<Texture2D>("cursor");
            _inputBox = new XNATextBox(new Rectangle(80, 57, 138, 19), Constants.FontSize08, caretTexture: cursorTexture)
            {
                LeftPadding = 5,
                DefaultText = " ",
                Text = " ",
                MaxChars = 12,
                Selected = true,
                TextColor = ColorConstants.LightBeigeText,
                Visible = true
            };
            _inputBox.SetParentControl(this);
            dispatcher.Subscriber = _inputBox;

            for (int i = 0; i < _arrowButtons.Length; ++i)
            {
                var btn = new XNAButton(_charCreateSheet,
                    new Vector2(196, 85 + i * 26),
                    new Rectangle(185, 38, 19, 19),
                    new Rectangle(206, 38, 19, 19));
                btn.OnClick += ArrowButtonClick;
                btn.SetParentControl(this);
                _arrowButtons[i] = btn;
            }

            _characterControl = new CreateCharacterControl(rendererFactory)
            {
                DrawPosition = new Vector2(235, 58)
            };
            _characterControl.SetParentControl(this);

            _srcRectangles[0] = new Rectangle(0, 38, 23, 19);
            _srcRectangles[1] = new Rectangle(0, 19, 23, 19);
            _srcRectangles[2] = new Rectangle(0, 0, 23, 19);
            _srcRectangles[3] = new Rectangle(46, 38, 23, 19);

            _ok = new XNAButton(eoDialogButtonService.SmallButtonSheet,
                new Vector2(157, 195),
                eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok),
                eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok));
            _ok.OnClick += (s, e) => ClickOk();
            _ok.SetParentControl(this);

            _cancel = new XNAButton(eoDialogButtonService.SmallButtonSheet,
                new Vector2(250, 195),
                eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel));
            _cancel.OnClick += (s, e) => Close(XNADialogResult.Cancel);
            _cancel.SetParentControl(this);

            CenterInGameView();
        }

        public override void Initialize()
        {
            _characterControl.Initialize();

            _inputBox.Initialize();
            foreach (var button in _arrowButtons)
                button.Initialize();

            _ok.Initialize();
            _cancel.Initialize();

            base.Initialize();
        }

        protected override void OnDrawControl(GameTime gt)
        {
            base.OnDrawControl(gt);

            _spriteBatch.Begin();

            for (int i = 0; i < 4; ++i)
            {
                _spriteBatch.Draw(_charCreateSheet,
                    new Vector2(170 + DrawPositionWithParentOffset.X,
                                84 + i*27 + DrawPositionWithParentOffset.Y),
                    _srcRectangles[i], Color.White);
            }

            _spriteBatch.End();
        }

        private void ArrowButtonClick(object sender, EventArgs e)
        {
            if (sender == _arrowButtons[0])
            {
                _characterControl.NextGender();
                _srcRectangles[0] = new Rectangle(0 + 23 * RenderProperties.Gender, 38, 23, 19);
            }
            else if (sender == _arrowButtons[1])
            {
                _characterControl.NextHairStyle();
                if (RenderProperties.HairStyle == 0) //skip bald
                    _characterControl.NextHairStyle();

                _srcRectangles[1] = new Rectangle(0 + 23 * (RenderProperties.HairStyle - 1), 19, 23, 19);
            }
            else if (sender == _arrowButtons[2])
            {
                _characterControl.NextHairColor();
                _srcRectangles[2] = new Rectangle(0 + 23 * RenderProperties.HairColor, 0, 23, 19);
            }
            else if (sender == _arrowButtons[3])
            {
                _characterControl.NextRace();
                _srcRectangles[3] = new Rectangle(46 + 23 * RenderProperties.Race, 38, 23, 19);
            }
        }

        private void ClickOk()
        {
            if (_inputBox.Text.Length < 4)
            {
                var messageBox = _messageBoxFactory.CreateMessageBox(DialogResourceID.CHARACTER_CREATE_NAME_TOO_SHORT,
                                                                     EODialogButtons.Ok,
                                                                     EOMessageBoxStyle.SmallDialogLargeHeader);
                messageBox.ShowDialog();
            }
            else
            {
                Close(XNADialogResult.OK);
            }
        }
    }
}
