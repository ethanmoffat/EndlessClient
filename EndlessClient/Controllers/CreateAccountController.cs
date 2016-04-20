// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Data.AccountCreation;

namespace EndlessClient.Controllers
{
	public class CreateAccountController : ICreateAccountController
	{
		private readonly ICreateAccountDialogDisplayActions _createAccountDialogDisplayActions;
		private readonly ICreateAccountActions _createAccountActions;
		private readonly IGameStateActions _gameStateActions;

		public CreateAccountController(ICreateAccountDialogDisplayActions createAccountDialogDisplayActions,
									   ICreateAccountActions createAccountActions,
									   IGameStateActions gameStateActions)
		{
			_createAccountDialogDisplayActions = createAccountDialogDisplayActions;
			_createAccountActions = createAccountActions;
			_gameStateActions = gameStateActions;
		}

		public async Task CreateAccount(ICreateAccountParameters createAccountParameters)
		{
			var paramsValidationResult = _createAccountActions.CheckAccountCreateParameters(createAccountParameters);
			if(paramsValidationResult.FaultingParameter != WhichParameter.None)
			{
				_createAccountDialogDisplayActions.ShowParameterError(paramsValidationResult);
				return;
			}

			var nameResult = await _createAccountActions.CheckAccountNameWithServer(createAccountParameters.AccountName);
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

			var accountResult = await _createAccountActions.CreateAccount(createAccountParameters);
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