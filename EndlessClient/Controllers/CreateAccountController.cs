// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Data.Account;

namespace EndlessClient.Controllers
{
	public class CreateAccountController : ICreateAccountController
	{
		private readonly ICreateAccountDialogDisplayActions _createAccountDialogDisplayActions;
		private readonly IAccountActions _accountActions;
		private readonly IGameStateActions _gameStateActions;

		public CreateAccountController(ICreateAccountDialogDisplayActions createAccountDialogDisplayActions,
									   IAccountActions accountActions,
									   IGameStateActions gameStateActions)
		{
			_createAccountDialogDisplayActions = createAccountDialogDisplayActions;
			_accountActions = accountActions;
			_gameStateActions = gameStateActions;
		}

		public async Task CreateAccount(ICreateAccountParameters createAccountParameters)
		{
			var paramsValidationResult = _accountActions.CheckAccountCreateParameters(createAccountParameters);
			if(paramsValidationResult.FaultingParameter != WhichParameter.None)
			{
				_createAccountDialogDisplayActions.ShowParameterError(paramsValidationResult);
				return;
			}

			var nameResult = await _accountActions.CheckAccountNameWithServer(createAccountParameters.AccountName);
			if (nameResult != AccountReply.Continue)
			{
				_createAccountDialogDisplayActions.ShowServerError(nameResult);
				return;
			}

			try
			{
				await _createAccountDialogDisplayActions.ShowAccountCreatePendingDialog();
			}
			catch (OperationCanceledException) { return; }

			var accountResult = await _accountActions.CreateAccount(createAccountParameters);
			if (accountResult != AccountReply.Created)
			{
				_createAccountDialogDisplayActions.ShowServerError(accountResult);
				return;
			}

			_gameStateActions.ChangeToState(GameStates.Initial);
			_createAccountDialogDisplayActions.ShowSuccessMessage();
		}
	}
}