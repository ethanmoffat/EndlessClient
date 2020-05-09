using System;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EOLib;
using EOLib.Domain.Account;
using EOLib.Localization;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
    [MappedType(BaseType = typeof(IAccountDialogDisplayActions))]
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

        public async Task<XNADialogResult> ShowCreatePendingDialog()
        {
            var progress = _createAccountProgressDialogFactory.BuildCreateAccountProgressDialog();
            return await progress.ShowDialogAsync();
        }

        public async Task<Optional<IChangePasswordParameters>> ShowChangePasswordDialog()
        {
            using (var changePassword = _changePasswordDialogFactory.BuildChangePasswordDialog())
            {
                var result = await changePassword.ShowDialogAsync();
                return result != XNADialogResult.OK
                    ? Optional<IChangePasswordParameters>.Empty
                    : new Optional<IChangePasswordParameters>(changePassword.Result);
            }
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
                case AccountReply.ChangeSuccess: message = DialogResourceID.CHANGE_PASSWORD_SUCCESS; break;
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