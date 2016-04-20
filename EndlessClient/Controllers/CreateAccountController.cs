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
		private readonly ICreateAccountActions _createAccountActions;

		public CreateAccountController(IEOMessageBoxFactory eoMessageBoxFactory,
									   ICreateAccountActions createAccountActions)
		{
			_eoMessageBoxFactory = eoMessageBoxFactory;
			_createAccountActions = createAccountActions;
		}

		public void CreateAccount(ICreateAccountParameters createAccountParameters)
		{
			var paramsValidationResult = _createAccountActions.CheckAccountCreateParameters(createAccountParameters);
			if(paramsValidationResult.FaultingParameter != WhichParameter.None)
			{
				_eoMessageBoxFactory.CreateMessageBox(
					paramsValidationResult.ErrorString,
					XNADialogButtons.Ok,
					EOMessageBoxStyle.SmallDialogLargeHeader);
				return;
			}

			//todo: do actual account creation logic
		}
	}
}