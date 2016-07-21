// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using EndlessClient.Content;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
using EndlessClient.Input;
using EOLib;
using EOLib.Domain.Account;
using EOLib.Domain.Login;
using EOLib.Graphics;
using EOLib.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Dialogs
{
    public class ChangePasswordDialog : EODialogBase
    {
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IPlayerInfoProvider _playerInfoProvider;
        private readonly XNATextBox[] _inputBoxes;

        private readonly TextBoxClickEventHandler _clickEventHandler;
        private readonly TextBoxTabEventHandler _tabEventHandler;

        private readonly TaskCompletionSource<XNADialogResult> _dialogResultCompletionSource;

        private string Username { get { return _inputBoxes[0].Text; } }
        private string OldPassword { get { return _inputBoxes[1].Text; } }
        private string NewPassword { get { return _inputBoxes[2].Text; } }
        private string ConfirmPassword { get { return _inputBoxes[3].Text; } }

        public ChangePasswordDialog(INativeGraphicsManager nativeGraphicsManager,
                                    IGraphicsDeviceProvider graphicsDeviceProvider,
                                    IGameStateProvider gameStateProvider,
                                    IContentManagerProvider contentManagerProvider,
                                    IEOMessageBoxFactory eoMessageBoxFactory,
                                    IKeyboardDispatcherProvider keyboardDispatcherProvider,
                                    IPlayerInfoProvider playerInfoProvider)
            : base(nativeGraphicsManager)
        {
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _playerInfoProvider = playerInfoProvider;
            var dispatcher = keyboardDispatcherProvider.Dispatcher;

            bgTexture = nativeGraphicsManager.TextureFromResource(GFXTypes.PreLoginUI, 21);
            _setSize(bgTexture.Width, bgTexture.Height);

            var cursorTexture = contentManagerProvider.Content.Load<Texture2D>("Cursor");

            _inputBoxes = new XNATextBox[4];
            for (int i = 0; i < _inputBoxes.Length; ++i)
            {
                var tb = new XNATextBox(new Rectangle(198, 60 + i * 30, 137, 19), cursorTexture, Constants.FontSize08)
                {
                    LeftPadding = 5,
                    DefaultText = " ",
                    MaxChars = i == 0 ? 16 : 12,
                    PasswordBox = i > 1,
                    Selected = i == 0,
                    TextColor = ColorConstants.LightBeigeText,
                    Visible = true
                };
                tb.SetParent(this);
                _inputBoxes[i] = tb;
            }

            _clickEventHandler = new TextBoxClickEventHandler(dispatcher, _inputBoxes);
            _tabEventHandler = new TextBoxTabEventHandler(dispatcher, _inputBoxes);

            dispatcher.Subscriber.Selected = false;
            dispatcher.Subscriber = _inputBoxes[0];
            dispatcher.Subscriber.Selected = true;

            var ok = new XNAButton(smallButtonSheet, new Vector2(157, 195), _getSmallButtonOut(SmallButton.Ok), _getSmallButtonOver(SmallButton.Ok));
            ok.OnClick += (s, e) => Close(ok, XNADialogResult.OK);
            ok.SetParent(this);
            dlgButtons.Add(ok);

            var cancel = new XNAButton(smallButtonSheet, new Vector2(250, 195), _getSmallButtonOut(SmallButton.Cancel), _getSmallButtonOver(SmallButton.Cancel));
            cancel.OnClick += (s, e) => Close(cancel, XNADialogResult.Cancel);
            cancel.SetParent(this);
            dlgButtons.Add(cancel);

            DialogClosing += OnDialogClosing;
            _dialogResultCompletionSource = new TaskCompletionSource<XNADialogResult>();

            CenterAndFixDrawOrder(graphicsDeviceProvider, gameStateProvider);
        }

        private void OnDialogClosing(object sender, CloseDialogEventArgs e)
        {
            if (e.Result == XNADialogResult.OK)
            {
                if (_inputBoxes.Any(tb => string.IsNullOrWhiteSpace(tb.Text)))
                {
                    e.CancelClose = true;
                    return;
                }

                if (Username != _playerInfoProvider.LoggedInAccountName)
                {
                    e.CancelClose = true;
                    _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.CHANGE_PASSWORD_MISMATCH);
                    return;
                }

                if (NewPassword.Length != ConfirmPassword.Length || NewPassword != ConfirmPassword)
                {
                    e.CancelClose = true;
                    _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.ACCOUNT_CREATE_PASSWORD_MISMATCH);
                    return;
                }

                if (NewPassword.Length < 6)
                {
                    e.CancelClose = true;
                    _eoMessageBoxFactory.CreateMessageBox(DialogResourceID.ACCOUNT_CREATE_PASSWORD_TOO_SHORT);
                    return;
                }
            }

            _dialogResultCompletionSource.SetResult(e.Result);
        }

        public new async Task<IChangePasswordParameters> Show()
        {
            var result = await _dialogResultCompletionSource.Task;
            if (result != XNADialogResult.OK)
                throw new OperationCanceledException();

            return new ChangePasswordParameters(Username, OldPassword, NewPassword);
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
