// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
using EOLib.Data.AccountCreation;
using XNAControls;

namespace EndlessClient.Controllers
{
	public class CreateAccountController : ICreateAccountController
	{
		private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
		private readonly ICreateAccountActions _createAccountActions;
		private readonly IGameStateActions _gameStateActions;

		public CreateAccountController(IEOMessageBoxFactory eoMessageBoxFactory,
									   ICreateAccountActions createAccountActions,
									   IGameStateActions gameStateActions)
		{
			_eoMessageBoxFactory = eoMessageBoxFactory;
			_createAccountActions = createAccountActions;
			_gameStateActions = gameStateActions;
		}

		public async Task CreateAccount(ICreateAccountParameters createAccountParameters)
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

			var nameResult = await _createAccountActions.CheckAccountNameWithServer(createAccountParameters.AccountName);
			if (nameResult != AccountReply.Continue)
			{
				//show dialog: name exists or name not approved
				return;
			}

			try
			{
				await _createAccountActions.ShowAccountCreatePendingDialog();
			}
			catch (OperationCanceledException) { return; }

			var accountResult = await _createAccountActions.CreateAccount(createAccountParameters);
			if (accountResult != AccountReply.Created)
			{
				//show dialog: some error from server, result should always be 'created' here
				return;
			}

			_gameStateActions.ChangeToState(GameStates.Initial);
			//show dialog: your account has been created
		}
	}
}