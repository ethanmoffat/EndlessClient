// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EndlessClient.Dialogs.Factories;
using EOLib;
using EOLib.Data.Account;
using EOLib.IO.Services;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
	public class CreateAccountDialogDisplayActions : ICreateAccountDialogDisplayActions
	{
		private readonly ILocalizedStringService _localizedStringService;
		private readonly ICreateAccountWarningDialogFactory _createAccountWarningDialogFactory;
		private readonly ICreateAccountProgressDialogFactory _createAccountProgressDialogFactory;
		private readonly IEOMessageBoxFactory _eoMessageBoxFactory;

		public CreateAccountDialogDisplayActions(ILocalizedStringService localizedStringService,
												 ICreateAccountWarningDialogFactory createAccountWarningDialogFactory,
												 ICreateAccountProgressDialogFactory createAccountProgressDialogFactory,
												 IEOMessageBoxFactory eoMessageBoxFactory)
		{
			_localizedStringService = localizedStringService;
			_createAccountWarningDialogFactory = createAccountWarningDialogFactory;
			_createAccountProgressDialogFactory = createAccountProgressDialogFactory;
			_eoMessageBoxFactory = eoMessageBoxFactory;
		}

		public void ShowInitialWarningDialog()
		{
			var message = string.Format("{0}\n\n{1}\n\n{2}",
				_localizedStringService.GetString(DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_1),
				_localizedStringService.GetString(DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_2),
				_localizedStringService.GetString(DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_3));

			_createAccountWarningDialogFactory.ShowCreateAccountWarningDialog(message);
		}

		public async Task ShowAccountCreatePendingDialog()
		{
			var progress = _createAccountProgressDialogFactory.BuildCreateAccountProgressDialog();
			await progress.WaitForCompletion();
		}

		public void ShowParameterError(CreateAccountParameterResult validationResult)
		{
			_eoMessageBoxFactory.CreateMessageBox(
				validationResult.ErrorString,
				XNADialogButtons.Ok,
				EOMessageBoxStyle.SmallDialogLargeHeader);
		}

		public void ShowServerError(AccountReply serverError)
		{
			DATCONST1 message;
			switch (serverError)
			{
				case AccountReply.Exists: message = DATCONST1.ACCOUNT_CREATE_NAME_EXISTS; break;
				case AccountReply.NotApproved: message = DATCONST1.ACCOUNT_CREATE_NAME_NOT_APPROVED; break;
				case AccountReply.Created: message = DATCONST1.ACCOUNT_CREATE_SUCCESS_WELCOME; break;
				case AccountReply.ChangeFailed: message = DATCONST1.CHANGE_PASSWORD_MISMATCH; break;
				case AccountReply.ChangeSuccess: message = DATCONST1.CHANGE_PASSWORD_SUCCESS; break;
				default: throw new ArgumentOutOfRangeException("serverError", serverError, null);
			}

			_eoMessageBoxFactory.CreateMessageBox(
				message,
				XNADialogButtons.Ok,
				EOMessageBoxStyle.SmallDialogLargeHeader);
		}

		public void ShowSuccessMessage()
		{
			ShowServerError(AccountReply.Created);
		}
	}
}