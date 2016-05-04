// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Domain.BLL;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EndlessClient.Controllers
{
	public class LoginController : ILoginController
	{
		private readonly ILoginActions _loginActions;
		private readonly IGameStateActions _gameStateActions;
		private readonly IErrorDialogDisplayAction _errorDisplayAction;
		private readonly ISafeInBandNetworkOperationFactory _networkOperationFactory;

		public LoginController(ILoginActions loginActions,
							   IGameStateActions gameStateActions,
							   IErrorDialogDisplayAction errorDisplayAction,
							   ISafeInBandNetworkOperationFactory networkOperationFactory)
		{
			_loginActions = loginActions;
			_gameStateActions = gameStateActions;
			_errorDisplayAction = errorDisplayAction;
			_networkOperationFactory = networkOperationFactory;
		}

		public async Task LoginToAccount(ILoginParameters loginParameters)
		{
			if (!_loginActions.LoginParametersAreValid(loginParameters))
				return;

			var loginToServerOperation = _networkOperationFactory.CreateSafeOperation(
				async () => await _loginActions.LoginToServer(loginParameters),
				SetInitialStateAndShowError, SetInitialStateAndShowError);

			if (!await loginToServerOperation.Invoke())
				return;
			var reply = loginToServerOperation.Result;

			if (reply == LoginReply.Ok)
				_gameStateActions.ChangeToState(GameStates.LoggedIn);
			else
				_errorDisplayAction.ShowLoginError(reply);
		}

		public async Task LoginToCharacter(ICharacter character)
		{
			var requestCharacterLoginOperation = _networkOperationFactory.CreateSafeOperation(
				async () => await _loginActions.RequestCharacterLogin(character),
				SetInitialStateAndShowError, SetInitialStateAndShowError);

			if (!await requestCharacterLoginOperation.Invoke())
				return;

			//show logging in / connection pending dialog
			//wait 5 seconds

			//request files 1 at a time (if required)
			//set dialog title appropriately at each of those different states

			//complete login and set dialog title

			//close dialog
			//transition to in-game state
		}

		private void SetInitialStateAndShowError(NoDataSentException ex)
		{
			_gameStateActions.ChangeToState(GameStates.Initial);
			_errorDisplayAction.ShowException(ex);
		}

		private void SetInitialStateAndShowError(EmptyPacketReceivedException ex)
		{
			_gameStateActions.ChangeToState(GameStates.Initial);
			_errorDisplayAction.ShowException(ex);
		}
	}
}