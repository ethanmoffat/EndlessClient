using System;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Account;
using EOLib.Localization;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
    [AutoMappedType]
    public class AccountDialogDisplayActions : IAccountDialogDisplayActions
    {
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ICreateAccountWarningDialogFactory _createAccountWarningDialogFactory;
        private readonly ICreateAccountProgressDialogFactory _createAccountProgressDialogFactory;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IChangePasswordDialogFactory _changePasswordDialogFactory;

        public AccountDialogDisplayActions(ILocalizedStringFinder localizedStringFinder,
                                           ICreateAccountWarningDialogFactory createAccountWarningDialogFactory,
                                           ICreateAccountProgressDialogFactory createAccountProgressDialogFactory,
                                           IEOMessageBoxFactory eoMessageBoxFactory,
                                           IChangePasswordDialogFactory changePasswordDialogFactory)
        {
            _localizedStringFinder = localizedStringFinder;
            _createAccountWarningDialogFactory = createAccountWarningDialogFactory;
            _createAccountProgressDialogFactory = createAccountProgressDialogFactory;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _changePasswordDialogFactory = changePasswordDialogFactory;
        }

        public void ShowInitialCreateWarningDialog()
        {
            var message =
                $"{_localizedStringFinder.GetString(EOResourceID.ACCOUNT_CREATE_WARNING_DIALOG_1)}\n\n{_localizedStringFinder.GetString(EOResourceID.ACCOUNT_CREATE_WARNING_DIALOG_2)}\n\n{_localizedStringFinder.GetString(EOResourceID.ACCOUNT_CREATE_WARNING_DIALOG_3)}";

            var dialog = _createAccountWarningDialogFactory.ShowCreateAccountWarningDialog(message);
            dialog.ShowDialog();
        }

        public Task<XNADialogResult> ShowCreatePendingDialog()
        {
            var progress = _createAccountProgressDialogFactory.BuildCreateAccountProgressDialog();
            return progress.ShowDialogAsync();
        }

        public Task<Option<IChangePasswordParameters>> ShowChangePasswordDialog()
        {
            var changePassword = _changePasswordDialogFactory.BuildChangePasswordDialog();
            return changePassword.ShowDialogAsync()
                .ContinueWith(showDialogTask => showDialogTask.Result.SomeWhen(x => x == XNADialogResult.OK).Map(x => changePassword.Result));
        }

        public void ShowCreateParameterValidationError(CreateAccountParameterResult validationResult)
        {
            var messageBox = _eoMessageBoxFactory.CreateMessageBox(
                validationResult.ErrorString,
                EODialogButtons.Ok,
                EOMessageBoxStyle.SmallDialogLargeHeader);
            messageBox.ShowDialog();
        }

        public void ShowCreateAccountServerError(AccountReply serverError)
        {
            DialogResourceID message;
            switch (serverError)
            {
                case AccountReply.Exists: message = DialogResourceID.ACCOUNT_CREATE_NAME_EXISTS; break;
                case AccountReply.NotApproved: message = DialogResourceID.ACCOUNT_CREATE_NAME_NOT_APPROVED; break;
                case AccountReply.Created: message = DialogResourceID.ACCOUNT_CREATE_SUCCESS_WELCOME; break;
                case AccountReply.ChangeFailed: message = DialogResourceID.CHANGE_PASSWORD_MISMATCH; break;
                case AccountReply.Changed: message = DialogResourceID.CHANGE_PASSWORD_SUCCESS; break;
                case AccountReply.RequestDenied: message = DialogResourceID.LOGIN_SERVER_COULD_NOT_PROCESS; break;
                default: throw new ArgumentOutOfRangeException(nameof(serverError), serverError, null);
            }

            var messageBox = _eoMessageBoxFactory.CreateMessageBox(
                message,
                EODialogButtons.Ok,
                EOMessageBoxStyle.SmallDialogLargeHeader);
            messageBox.ShowDialog();
        }

        public void ShowSuccessMessage()
        {
            ShowCreateAccountServerError(AccountReply.Created);
        }
    }
}
