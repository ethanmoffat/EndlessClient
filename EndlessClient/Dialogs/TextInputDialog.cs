using EndlessClient.Content;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EOLib;
using EOLib.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class TextInputDialog : BaseEODialog
    {
        private readonly IKeyboardDispatcherRepository _keyboardDispatcherRepository;

        private readonly IXNATextBox _inputBox;
        private readonly IKeyboardSubscriber _previousSubscriber;

        public string ResponseText => _inputBox.Text;

        public TextInputDialog(IGameStateProvider gameStateProvider,
                               INativeGraphicsManager nativeGraphicsManager,
                               IEODialogButtonService eoDialogButtonService,
                               IKeyboardDispatcherRepository keyboardDispatcherRepository,
                               IContentProvider contentProvider,
                               string prompt,
                               int maxInputChars = 12)
            : base(gameStateProvider)
        {
            _keyboardDispatcherRepository = keyboardDispatcherRepository;

            BackgroundTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PostLoginUI, 54);
            SetSize(BackgroundTexture.Width, BackgroundTexture.Height);

            var lblPrompt = new XNALabel(Constants.FontSize10)
            {
                AutoSize = false,
                DrawArea = new Rectangle(16, 20, 235, 49),
                ForeColor = ColorConstants.LightGrayDialogMessage,
                TextWidth = 230,
                Text = prompt
            };
            lblPrompt.Initialize();
            lblPrompt.SetParentControl(this);

            _inputBox = new XNATextBox(new Rectangle(37, 74, 192, 19), Constants.FontSize08, caretTexture: contentProvider.Textures[ContentProvider.Cursor])
            {
                MaxChars = maxInputChars,
                LeftPadding = 4,
                TextColor = ColorConstants.LightBeigeText
            };
            _inputBox.Initialize();
            _inputBox.SetParentControl(this);

            _previousSubscriber = _keyboardDispatcherRepository.Dispatcher.Subscriber;
            _keyboardDispatcherRepository.Dispatcher.Subscriber = _inputBox;

            var ok = new XNAButton(eoDialogButtonService.SmallButtonSheet,
                new Vector2(41, 103),
                eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok),
                eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok));
            ok.OnClick += (_, _) => Close(XNADialogResult.OK);
            ok.SetParentControl(this);

            var cancel = new XNAButton(eoDialogButtonService.SmallButtonSheet,
                new Vector2(134, 103),
                eoDialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                eoDialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel));
            cancel.OnClick += (_, _) => Close(XNADialogResult.Cancel);
            cancel.SetParentControl(this);

            DialogClosed += (_, _) => _keyboardDispatcherRepository.Dispatcher.Subscriber = _previousSubscriber;

            CenterInGameView();
            DrawPosition += new Vector2(0, 17);
        }
    }
}
