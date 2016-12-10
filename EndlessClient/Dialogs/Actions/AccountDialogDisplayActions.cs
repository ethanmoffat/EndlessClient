// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Account;
using EOLib.Localization;
using XNAControls.Old;

namespace EndlessClient.Dialogs.Actions
{
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
            var message = string.Format("{0}\n\n{1}\n\n{2}",
                _localizedStringFinder.GetString(EOResourceID.ACCOUNT_CREATE_WARNING_DIALOG_1),
                _localizedStringFinder.GetString(EOResourceID.ACCOUNT_CREATE_WARNING_DIALOG_2),
                _localizedStringFinder.GetString(EOResourceID.ACCOUNT_CREATE_WARNING_DIALOG_3));

            var dialog = _createAccountWarningDialogFactory.ShowCreateAccountWarningDialog(message);
            dialog.ShowDialog();
        }

        public async Task ShowCreatePendingDialog()
        {
            var progress = _createAccountProgressDialogFactory.BuildCreateAccountProgressDialog();
            await progress.WaitForCompletion();
        }

        public async Task<IChangePasswordParameters> ShowChangePasswordDialog()
        {
            var changePassword = _changePasswordDialogFactory.BuildChangePasswordDialog();
            return await changePassword.Show();
        }

        public void ShowCreateParameterValidationError(CreateAccountParameterResult validationResult)
        {
            _eoMessageBoxFactory.CreateMessageBox(
                validationResult.ErrorString,
                EODialogButtons.Ok,
                EOMessageBoxStyle.SmallDialogLargeHeader);
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
                default: throw new ArgumentOutOfRangeException("serverError", serverError, null);
            }

            _eoMessageBoxFactory.CreateMessageBox(
                message,
                EODialogButtons.Ok,
                EOMessageBoxStyle.SmallDialogLargeHeader);
        }

        public void ShowSuccessMessage()
        {
            ShowCreateAccountServerError(AccountReply.Created);
        }
    }
}