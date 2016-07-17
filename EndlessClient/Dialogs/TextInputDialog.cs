// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using EOLib.Graphics;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class TextInputDialog : EODialogBase
    {
        private readonly XNATextBox m_inputBox;
        private readonly IKeyboardSubscriber previousSubscriber;

        public string ResponseText { get { return m_inputBox.Text; } }

        public TextInputDialog(string prompt, int maxInputChars = 12)
            : base((PacketAPI)null)
        {
            bgTexture = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 54);
            _setSize(bgTexture.Width, bgTexture.Height);

            XNALabel lblPrompt = new XNALabel(new Rectangle(16, 20, 235, 49), Constants.FontSize10)
            {
                AutoSize = false,
                ForeColor = ColorConstants.LightGrayDialogMessage,
                TextWidth = 230,
                RowSpacing = 3,
                Text = prompt
            };
            lblPrompt.SetParent(this);

            //set this back once the dialog is closed.
            previousSubscriber = ((EOGame)Game).Dispatcher.Subscriber;
            DialogClosing += (o, e) => ((EOGame)Game).Dispatcher.Subscriber = previousSubscriber;

            m_inputBox = new XNATextBox(new Rectangle(37, 74, 192, 19), EOGame.Instance.Content.Load<Texture2D>("cursor"), Constants.FontSize08)
            {
                MaxChars = maxInputChars,
                LeftPadding = 4,
                TextColor = ColorConstants.LightBeigeText
            };
            m_inputBox.SetParent(this);
            EOGame.Instance.Dispatcher.Subscriber = m_inputBox;

            XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(41, 103), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok)),
                cancel = new XNAButton(smallButtonSheet, new Vector2(134, 103), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
            ok.OnClick += (o, e) => Close(ok, XNADialogResult.OK);
            cancel.OnClick += (o, e) => Close(cancel, XNADialogResult.Cancel);
            ok.SetParent(this);
            cancel.SetParent(this);

            Center(Game.GraphicsDevice);
            DrawLocation = new Vector2(DrawLocation.X, 107);
            endConstructor(false);
        }

        public void SetAsKeyboardSubscriber()
        {
            ((EOGame)Game).Dispatcher.Subscriber = m_inputBox;
        }
    }
}
