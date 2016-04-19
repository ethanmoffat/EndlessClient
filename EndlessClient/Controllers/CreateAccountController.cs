// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
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
		private readonly IAccountCreateParameterProvider _accountCreateParameterProvider;

		public CreateAccountController(IEOMessageBoxFactory eoMessageBoxFactory,
									   IAccountCreateActions accountCreateActions,
									   IAccountCreateParameterProvider accountCreateParameterProvider)
		{
			_eoMessageBoxFactory = eoMessageBoxFactory;
			_accountCreateActions = accountCreateActions;
			_accountCreateParameterProvider = accountCreateParameterProvider;
		}

		public void CreateAccount()
		{
			if (_accountCreateParameterProvider.AccountCreateParameters == null)
				throw new InvalidOperationException("This controller should be called after the account create parameters are set");

			try
			{
				_accountCreateActions.CheckAccountCreateParameters();
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