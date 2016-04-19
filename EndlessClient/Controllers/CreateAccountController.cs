// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Factories;
using EOLib.Data.AccountCreation;
using XNAControls;

namespace EndlessClient.Controllers
{
	public class CreateAccountController : ICreateAccountController
	{
		private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
		private readonly IAccountCreateActions _accountCreateActions;

		public CreateAccountController(IEOMessageBoxFactory eoMessageBoxFactory,
									   IAccountCreateActions accountCreateActions)
		{
			_eoMessageBoxFactory = eoMessageBoxFactory;
			_accountCreateActions = accountCreateActions;
		}

		public void CreateAccount(IAccountCreateParameters accountCreateParameters)
		{
			try
			{
				_accountCreateActions.CheckAccountCreateParameters(accountCreateParameters);
			}
			catch (AccountCreateParameterException ex)
			{
				_eoMessageBoxFactory.CreateMessageBox(
					ex.Error,
					XNADialogButtons.Ok,
					EOMessageBoxStyle.SmallDialogLargeHeader);
				return;
			}

			//todo: do actual account creation logic
		}
	}
}