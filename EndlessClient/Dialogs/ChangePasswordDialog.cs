using System;
using System.Linq;
using EndlessClient.Content;
using EndlessClient.Dialogs.Factories;
using EndlessClient.Dialogs.Services;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Account;
using EOLib.Domain.Login;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Optional;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class ChangePasswordDialog : BaseEODialog
    {
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly IXnaControlSoundMapper _xnaControlSoundMapper;
        private readonly IXNATextBox[] _inputBoxes;
        private readonly IXNAButton _ok, _cancel;

        private readonly TextBoxClickEventHandler _clickEventHandler;
        private readonly TextBoxTabEventHandler _tabEventHandler;

        private string Username => _inputBoxes[0].Text;
        private string OldPassword => _inputBoxes[1].Text;
        private string NewPassword => _inputBoxes[2].Text;
        private string ConfirmPassword => _inputBoxes[3].Text;

        public IChangePasswordParameters Result => new ChangePasswordParameters(Username, OldPassword, NewPassword);

        public ChangePasswordDialog(INativeGraphicsManager nativeGraphicsManager,
                                    IGameStateProvider gameStateProvider,
                                    IContentProvider contentProvider,
                                    IEOMessageBoxFactory eoMessageBoxFactory,
                                    IKeyboardDispatcherProvider keyboardDispatcherProvider,
                                    IPlayerInfoProvider playerInfoProvider,
                                    IEODialogButtonService dialogButtonService,
                                    IXnaControlSoundMapper xnaControlSoundMapper)
            : base(nativeGraphicsManager, gameStateProvider)
        {
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _playerInfoProvider = playerInfoProvider;
            _xnaControlSoundMapper = xnaControlSoundMapper;

            var dispatcher = keyboardDispatcherProvider.Dispatcher;

            BackgroundTexture = GraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 21);

            var cursorTexture = contentProvider.Textures[ContentProvider.Cursor];

            _inputBoxes = new IXNATextBox[4];
            for (int i = 0; i < _inputBoxes.Length; ++i)
            {
                var tb = new XNATextBox(new Rectangle(198, 60 + i * 30, 137, 19), Constants.FontSize08, caretTexture: cursorTexture)
                {
                    LeftPadding = 5,
                    DefaultText = " ",
                    MaxChars = i == 0 ? 16 : 12,
                    PasswordBox = i > 1,
                    TextColor = ColorConstants.LightBeigeText
                };
                _inputBoxes[i] = tb;
            }

            _clickEventHandler = new TextBoxClickEventHandler(dispatcher, _inputBoxes);
            _tabEventHandler = new TextBoxTabEventHandler(dispatcher, _inputBoxes);

            dispatcher.Subscriber = _inputBoxes[0];

            _ok = new XNAButton(
                dialogButtonService.SmallButtonSheet,
                new Vector2(157, 195),
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Ok),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Ok));
            _ok.OnClick += OnButtonPressed;

            _cancel = new XNAButton(
                dialogButtonService.SmallButtonSheet,
                new Vector2(250, 195),
                dialogButtonService.GetSmallDialogButtonOutSource(SmallButton.Cancel),
                dialogButtonService.GetSmallDialogButtonOverSource(SmallButton.Cancel));
            _cancel.OnClick += (s, e) => Close(XNADialogResult.Cancel);

            CenterInGameView();
        }

        public override void Initialize()
        {
            foreach (var tb in _inputBoxes)
            {
                tb.Initialize();
                tb.SetParentControl(this);
                _xnaControlSoundMapper.BindSoundToControl(tb);
            }

            _ok.Initialize();
            _ok.SetParentControl(this);
            _xnaControlSoundMapper.BindSoundToControl(_ok, Option.Some(Audio.SoundEffectID.DialogButtonClick));

            _cancel.Initialize();
            _cancel.SetParentControl(this);
            _xnaControlSoundMapper.BindSoundToControl(_cancel, Option.Some(Audio.SoundEffectID.DialogButtonClick));

            base.Initialize();
        }

        private void OnButtonPressed(object sender, EventArgs e)
        {
            if (_inputBoxes.Any(tb => string.IsNullOrWhiteSpace(tb.Text)))
            {
                return;
            }

            if (Username != _playerInfoProvider.LoggedInAccountName)
            {
                var messageBox = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.CHANGE_PASSWORD_MISMATCH);
                messageBox.ShowDialog();
                return;
            }

            if (OldPassword != _playerInfoProvider.PlayerPassword)
            {
                var messageBox = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.CHANGE_PASSWORD_MISMATCH);
                messageBox.ShowDialog();
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                var messageBox = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.ACCOUNT_CREATE_PASSWORD_MISMATCH);
                messageBox.ShowDialog();
                return;
            }

            if (NewPassword.Length < 6)
            {
                var messageBox = _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.ACCOUNT_CREATE_PASSWORD_TOO_SHORT);
                messageBox.ShowDialog();
                return;
            }

            Close(XNADialogResult.OK);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _clickEventHandler.Dispose();
                _tabEventHandler.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
