// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Dialogs.Factories;
using EOLib;
using EOLib.IO.Services;

namespace EndlessClient.Dialogs.Actions
{
	public class CreateAccountDialogDisplayActions : ICreateAccountDialogDisplayActions
	{
		private readonly ILocalizedStringService _localizedStringService;
		private readonly ICreateAccountWarningDialogFactory _createAccountWarningDialogFactory;

		public CreateAccountDialogDisplayActions(ILocalizedStringService localizedStringService,
												ICreateAccountWarningDialogFactory createAccountWarningDialogFactory)
		{
			_localizedStringService = localizedStringService;
			_createAccountWarningDialogFactory = createAccountWarningDialogFactory;
		}

		public void ShowCreateAccountDialog()
		{
			var message = string.Format("{0}\n\n{1}\n\n{2}",
				_localizedStringService.GetString(DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_1),
				_localizedStringService.GetString(DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_2),
				_localizedStringService.GetString(DATCONST2.ACCOUNT_CREATE_WARNING_DIALOG_3));

			_createAccountWarningDialogFactory.ShowCreateAccountWarningDialog(message);
		}
	}
}