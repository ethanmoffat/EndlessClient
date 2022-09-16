using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.Input;
using EOLib;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class ItemTransferDialog : BaseEODialog
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

        private readonly Texture2D _backgroundTexture;
        private readonly Rectangle _backgroundTextureSource;

        private readonly Rectangle? _titleBarTextureSource;

        private readonly int _totalAmount;

        private readonly IXNALabel _descLabel;
        private readonly IXNATextBox _amount;
        private readonly IXNAButton _slider, _okButton, _cancelButton;

        public int SelectedAmount => int.Parse(_amount.Text);

        private readonly IKeyboardSubscriber _prevSubscriber;

        private bool _sliderDragging;

        public ItemTransferDialog(INativeGraphicsManager nativeGraphicsManager,
                                  IEODialogButtonService eoDialogButtonService,
                                  ILocalizedStringFinder localizedStringFinder,
                                  IContentProvider contentProvider,
                                  IKeyboardDispatcherRepository keyboardDispatcherRepository,
                                  string itemName,
                                  TransferType transferType,
                                  int totalAmount,
                                  EOResourceID message)
            : base(nativeGraphicsManager, isInGame: true)
        {
            if (!IsValidMessage(message))
                throw new ArgumentOutOfRangeException(nameof(message), "Use one of the approved messages.");

            _backgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 27);
            _backgroundTextureSource = new Rectangle(38, 0, 265, 170);

            // set so CenterInGameView works properly (expected BackgroundTexture to be set)
            BackgroundTexture = new Texture2D(Game.GraphicsDevice, _backgroundTextureSource.Width, _backgroundTextureSource.Height);

            SetSize(_backgroundTextureSource.Width, _backgroundTextureSource.Height);
            CenterInGameView();

            if (transferType != TransferType.DropItems)
            {
                _titleBarTextureSource = new Rectangle(40, 172 + ((int)transferType - 1) * 24, 241, 22);
            }

            _okButton = new XNAButton(eoDialogButtonService.SmallButtonSheet,
                new Vector2(60, 125), 
                eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok),
                eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok))
            {
                Visible = true
            };
            _okButton.OnClick += (s, e) => Close(XNADialogResult.OK);

            _cancelButton = new XNAButton(eoDialogButtonService.SmallButtonSheet,
                new Vector2(153, 125),
                eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel))
            {
                Visible = true
            };
            _cancelButton.OnClick += (s, e) => Close(XNADialogResult.Cancel);

            _descLabel = new XNALabel(Constants.FontSize10)
            {
                DrawArea = new Rectangle(20, 42, 231, 33),
                ForeColor = ColorConstants.LightGrayDialogMessage,
                TextWidth = 200,
                Text = $"{localizedStringFinder.GetString(EOResourceID.DIALOG_TRANSFER_HOW_MUCH)} {itemName} {localizedStringFinder.GetString(message)}"
            };

            //set the text box here
            //starting coords are 163, 97
            _amount = new XNATextBox(new Rectangle(163, 95, 77, 19), Constants.FontSize08, caretTexture: contentProvider.Textures[ContentProvider.Cursor])
            {
                Visible = true,
                Enabled = true,
                MaxChars = 8, //max drop/junk at a time will be 99,999,999
                TextColor = ColorConstants.LightBeigeText,
                Text = "1"
            };
            _amount.OnTextChanged += AmountTextChanged;

            _prevSubscriber = keyboardDispatcherRepository.Dispatcher.Subscriber;
            keyboardDispatcherRepository.Dispatcher.Subscriber = _amount;
            DialogClosed += (_, _) => keyboardDispatcherRepository.Dispatcher.Subscriber = _prevSubscriber;

            _totalAmount = totalAmount;

            //slider control
            var sliderTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 29);
            //5th index when 'out', 6th index when 'over'
            Rectangle sliderOutSource = new Rectangle(0, 15 * 5, 16, 15);
            Rectangle sliderOverSource = new Rectangle(0, 15 * 6, 16, 15);

            //starting coords are 25, 96
            _slider = new XNAButton(sliderTexture, new Vector2(25, 96), sliderOutSource, sliderOverSource);
            _slider.OnClickDrag += SliderClickDrag;
        }

        public override void Initialize()
        {
            _okButton.Initialize();
            _okButton.SetParentControl(this);

            _cancelButton.Initialize();
            _cancelButton.SetParentControl(this);

            _descLabel.Initialize();
            _descLabel.SetParentControl(this);

            _amount.Initialize();
            _amount.SetParentControl(this);

            _slider.Initialize();
            _slider.SetParentControl(this);

            base.Initialize();
        }

        protected override void OnDrawControl(GameTime gameTime)
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(_backgroundTexture, DrawAreaWithParentOffset, _backgroundTextureSource, Color.White);

            if (_titleBarTextureSource != null)
            {
                _spriteBatch.Draw(_backgroundTexture, DrawPositionWithParentOffset + new Vector2(10, 10), _titleBarTextureSource, Color.White);
            }

            _spriteBatch.End();

            base.OnDrawControl(gameTime);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BackgroundTexture.Dispose();
            }

            base.Dispose(disposing);
        }

        private void SliderClickDrag(object sender, EventArgs e)
        {
            _sliderDragging = true; //ignores updates to slider location during text change

            //range rectangle is 122, 15
            var sliderArea = new Rectangle(25, 96, 122 - _slider.DrawArea.Width, 15);
            var newX = CurrentMouseState.X - PreviousMouseState.X + (int)_slider.DrawPosition.X;

            if (newX < sliderArea.X)
                newX = sliderArea.X;
            else if (newX > sliderArea.Width + sliderArea.X)
                newX = sliderArea.Width + sliderArea.X;

            _slider.DrawPosition = new Vector2(newX, _slider.DrawPosition.Y);

            var ratio = (newX - sliderArea.X) / (float)sliderArea.Width;
            _amount.Text = ((int)Math.Round(ratio * _totalAmount) + 1).ToString();

            _sliderDragging = false;
        }

        private void AmountTextChanged(object sender, EventArgs e)
        {
            int amt = 0;
            if (_amount.Text != "" && (!int.TryParse(_amount.Text, out amt) || amt > _totalAmount))
            {
                amt = _totalAmount;
                _amount.Text = $"{_totalAmount}";
            }
            else if (_amount.Text != "" && amt < 0)
            {
                amt = 1;
                _amount.Text = $"{amt}";
            }

            if (!_sliderDragging)
            {
                if (amt <= 1)
                {
                    _slider.DrawPosition = new Vector2(25, 96);
                }
                else
                {
                    int xCoord = (int)Math.Round((amt / (double)_totalAmount) * (122 - _slider.DrawArea.Width));
                    _slider.DrawPosition = new Vector2(25 + xCoord, 96);
                }
            }
        }

        private static bool IsValidMessage(EOResourceID msg)
        {
            var name = Enum.GetName(typeof(EOResourceID), msg);
            return name.Contains("DIALOG_TRANSFER");
        }
    }

}
