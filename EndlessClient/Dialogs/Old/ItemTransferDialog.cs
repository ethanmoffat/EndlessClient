using System;
using EndlessClient.Dialogs.Services;
using EndlessClient.Old;
using EOLib;
using EOLib.Graphics;
using EOLib.Localization;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using XNAControls;
using XNAButton = XNAControls.Old.XNAButton;
using XNADialogResult = XNAControls.Old.XNADialogResult;
using XNALabel = XNAControls.Old.XNALabel;
using XNATextBox = XNAControls.Old.XNATextBox;

namespace EndlessClient.Dialogs.Old
{
    public class ItemTransferDialog : EODialogBase
    {
        public enum TransferType
        {
            DropItems,
            JunkItems,
            GiveItems,
            TradeItems,
            ShopTransfer,
            BankTransfer
        }

        private readonly Texture2D m_titleBarGfx;
        private readonly int m_totalAmount;

        private readonly XNATextBox m_amount;
        public int SelectedAmount => int.Parse(m_amount.Text);

        private readonly IKeyboardSubscriber m_prevSubscriber;

        private static bool s_sliderDragging;

        /// <summary>
        /// Create a new item transfer dialog
        /// </summary>
        /// <param name="itemName">Name of the item to be displayed</param>
        /// <param name="transferType">Which transfer is being done (controls title)</param>
        /// <param name="totalAmount">Maximum amount that can be transferred</param>
        /// <param name="message">Resource ID of message to control displayed text</param>
        public ItemTransferDialog(string itemName, TransferType transferType, int totalAmount, EOResourceID message = EOResourceID.DIALOG_TRANSFER_DROP)
            : base((PacketAPI)null)
        {
            _validateMessage(message);

            Texture2D weirdSpriteSheet = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 27);
            Rectangle sourceArea = new Rectangle(38, 0, 265, 170);

            //get bgTexture
            Color[] textureData = new Color[sourceArea.Width * sourceArea.Height];
            bgTexture = new Texture2D(Game.GraphicsDevice, sourceArea.Width, sourceArea.Height);
            weirdSpriteSheet.GetData(0, sourceArea, textureData, 0, textureData.Length);
            bgTexture.SetData(textureData);

            //get the title bar - for when it isn't drop items
            if (transferType != TransferType.DropItems)
            {
                Rectangle titleBarArea = new Rectangle(40, 172 + ((int)transferType - 1) * 24, 241, 22);
                Color[] titleBarData = new Color[titleBarArea.Width * titleBarArea.Height];
                m_titleBarGfx = new Texture2D(Game.GraphicsDevice, titleBarArea.Width, titleBarArea.Height);
                weirdSpriteSheet.GetData(0, titleBarArea, titleBarData, 0, titleBarData.Length);
                m_titleBarGfx.SetData(titleBarData);
            }

            //set the buttons here

            //ok/cancel buttons
            XNAButton ok = new XNAButton(smallButtonSheet, new Vector2(60, 125), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok))
            {
                Visible = true
            };
            ok.OnClick += (s, e) => Close(ok, XNADialogResult.OK);
            ok.SetParent(this);
            dlgButtons.Add(ok);

            XNAButton cancel = new XNAButton(smallButtonSheet, new Vector2(153, 125), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel))
            {
                Visible = true
            };
            cancel.OnClick += (s, e) => Close(cancel, XNADialogResult.Cancel);
            cancel.SetParent(this);
            dlgButtons.Add(cancel);

            XNALabel descLabel = new XNALabel(new Rectangle(20, 42, 231, 33), Constants.FontSize10)
            {
                ForeColor = ColorConstants.LightGrayDialogMessage,
                TextWidth = 200,
                Text =
                    $"{OldWorld.GetString(EOResourceID.DIALOG_TRANSFER_HOW_MUCH)} {itemName} {OldWorld.GetString(message)}"
            };
            descLabel.SetParent(this);

            //set the text box here
            //starting coords are 163, 97
            m_amount = new XNATextBox(new Rectangle(163, 95, 77, 19), Game.Content.Load<Texture2D>("cursor"), Constants.FontSize08)
            {
                Visible = true,
                Enabled = true,
                MaxChars = 8, //max drop/junk at a time will be 99,999,999
                TextColor = ColorConstants.LightBeigeText,
                Text = "1"
            };
            m_amount.SetParent(this);
            m_prevSubscriber = EOGame.Instance.Dispatcher.Subscriber;
            EOGame.Instance.Dispatcher.Subscriber = m_amount;
            DialogClosing += (o, e) => EOGame.Instance.Dispatcher.Subscriber = m_prevSubscriber;

            m_totalAmount = totalAmount;

            //slider control
            Texture2D src = ((EOGame)Game).GFXManager.TextureFromResource(GFXTypes.PostLoginUI, 29);
            //5th index when 'out', 6th index when 'over'
            Rectangle outText = new Rectangle(0, 15 * 5, 16, 15);
            Rectangle ovrText = new Rectangle(0, 15 * 6, 16, 15);
            Color[] outData = new Color[16 * 15];
            Color[] ovrData = new Color[16 * 15];
            Texture2D[] sliderTextures = new Texture2D[2];

            src.GetData(0, outText, outData, 0, outData.Length);
            src.GetData(0, ovrText, ovrData, 0, ovrData.Length);
            (sliderTextures[0] = new Texture2D(Game.GraphicsDevice, 16, 15)).SetData(outData);
            (sliderTextures[1] = new Texture2D(Game.GraphicsDevice, 16, 15)).SetData(ovrData);

            //starting coords are 25, 96; range rectangle is 122, 15
            XNAButton slider = new XNAButton(sliderTextures, new Vector2(25, 96));
            slider.OnClickDrag += (o, e) =>
            {
                s_sliderDragging = true; //ignores updates to slider location during text change
                MouseState st = Mouse.GetState();
                Rectangle sliderArea = new Rectangle(25, 96, 122 - slider.DrawArea.Width, 15);
                int newX = (st.X - PreviousMouseState.X) + (int)slider.DrawLocation.X;
                if (newX < sliderArea.X) newX = sliderArea.X;
                else if (newX > sliderArea.Width + sliderArea.X) newX = sliderArea.Width + sliderArea.X;
                slider.DrawLocation = new Vector2(newX, slider.DrawLocation.Y); //unchanged y coordinate, slides along x-axis

                float ratio = (newX - sliderArea.X) / (float)sliderArea.Width;
                m_amount.Text = ((int)Math.Round(ratio * m_totalAmount) + 1).ToString();
                s_sliderDragging = false;
            };
            slider.SetParent(this);

            m_amount.OnTextChanged += (sender, args) =>
            {
                int amt = 0;
                if (m_amount.Text != "" && (!int.TryParse(m_amount.Text, out amt) || amt > m_totalAmount))
                {
                    amt = m_totalAmount;
                    m_amount.Text = $"{m_totalAmount}";
                }
                else if (m_amount.Text != "" && amt < 0)
                {
                    amt = 1;
                    m_amount.Text = $"{amt}";
                }

                if (s_sliderDragging) return; //slider is being dragged - don't move its position

                //adjust the slider (created after m_amount) when the text changes
                if (amt <= 1) //NOT WORKING
                {
                    slider.DrawLocation = new Vector2(25, 96);
                }
                else
                {
                    int xCoord = (int)Math.Round((amt / (double)m_totalAmount) * (122 - slider.DrawArea.Width));
                    slider.DrawLocation = new Vector2(25 + xCoord, 96);
                }
            };

            _setSize(bgTexture.Width, bgTexture.Height);
            DrawLocation = new Vector2(Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - bgTexture.Width / 2, 40); //only centered horizontally
            endConstructor(false);
        }

        public override void Draw(GameTime gt)
        {
            base.Draw(gt);

            if (m_titleBarGfx != null)
            {
                SpriteBatch.Begin();
                SpriteBatch.Draw(m_titleBarGfx, new Vector2(DrawAreaWithOffset.X + 10, DrawAreaWithOffset.Y + 10), Color.White);
                SpriteBatch.End();
            }
        }

        private void _validateMessage(EOResourceID msg)
        {
            switch (msg)
            {
                case EOResourceID.DIALOG_TRANSFER_DROP:
                case EOResourceID.DIALOG_TRANSFER_GIVE:
                case EOResourceID.DIALOG_TRANSFER_JUNK:
                case EOResourceID.DIALOG_TRANSFER_BUY:
                case EOResourceID.DIALOG_TRANSFER_SELL:
                case EOResourceID.DIALOG_TRANSFER_TRANSFER:
                case EOResourceID.DIALOG_TRANSFER_DEPOSIT:
                case EOResourceID.DIALOG_TRANSFER_WITHDRAW:
                case EOResourceID.DIALOG_TRANSFER_OFFER:
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(msg), "Use one of the approved messages.");
            }
        }
    }

}
