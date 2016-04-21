// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Data.Account;
using EOLib.Net.Communication;
using EOLib.Net.Connection;

namespace EndlessClient.Controllers
{
	public class CreateAccountController : ICreateAccountController
	{
		private readonly ICreateAccountDialogDisplayActions _createAccountDialogDisplayActions;
		private readonly IErrorDialogDisplayAction _errorDisplayAction;
		private readonly IAccountActions _accountActions;
		private readonly IGameStateActions _gameStateActions;
		private readonly INetworkConnectionActions _networkConnectionActions;
		private readonly IBackgroundReceiveActions _backgroundReceiveActions;

		public CreateAccountController(ICreateAccountDialogDisplayActions createAccountDialogDisplayActions,
									   IErrorDialogDisplayAction errorDisplayAction,
									   IAccountActions accountActions,
									   IGameStateActions gameStateActions,
									   INetworkConnectionActions networkConnectionActions,
									   IBackgroundReceiveActions backgroundReceiveActions)
		{
			_createAccountDialogDisplayActions = createAccountDialogDisplayActions;
			_errorDisplayAction = errorDisplayAction;
			_accountActions = accountActions;
			_gameStateActions = gameStateActions;
			_networkConnectionActions = networkConnectionActions;
			_backgroundReceiveActions = backgroundReceiveActions;
		}

		public async Task CreateAccount(ICreateAccountParameters createAccountParameters)
		{
			var paramsValidationResult = _accountActions.CheckAccountCreateParameters(createAccountParameters);
			if(paramsValidationResult.FaultingParameter != WhichParameter.None)
			{
				_createAccountDialogDisplayActions.ShowParameterError(paramsValidationResult);
				return;
			}

			try
			{
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
				catch (OperationCanceledException)
				{
					return;
				}

				var accountResult = await _accountActions.CreateAccount(createAccountParameters);
				if (accountResult != AccountReply.Created)
				{
					_createAccountDialogDisplayActions.ShowServerError(accountResult);
					return;
				}
			}
			catch (NoDataSentException)
			{
				SetInitialStateAndShowError();
				DisconnectAndStopReceiving();
				return;
			}
			catch (EmptyPacketReceivedException)
			{
				SetInitialStateAndShowError();
				DisconnectAndStopReceiving();
				return;
			}

			_gameStateActions.ChangeToState(GameStates.Initial);
			_createAccountDialogDisplayActions.ShowSuccessMessage();
		}

		private void SetInitialStateAndShowError()
		{
			_gameStateActions.ChangeToState(GameStates.Initial);
			_errorDisplayAction.ShowError(ConnectResult.SocketError);
		}

		private void DisconnectAndStopReceiving()
		{
			_backgroundReceiveActions.CancelBackgroundReceiveLoop();
			_networkConnectionActions.DisconnectFromServer();
		}
	}
}