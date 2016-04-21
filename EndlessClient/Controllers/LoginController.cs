// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Data.Login;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EndlessClient.Controllers
{
	public class LoginController : ILoginController
	{
		private readonly ILoginActions _loginActions;
		private readonly IGameStateActions _gameStateActions;
		private readonly IErrorDialogDisplayAction _errorDisplayAction;

		public LoginController(ILoginActions loginActions,
							   IGameStateActions gameStateActions,
							   IErrorDialogDisplayAction errorDisplayAction)
		{
			_loginActions = loginActions;
			_gameStateActions = gameStateActions;
			_errorDisplayAction = errorDisplayAction;
		}

		public async Task LoginToAccount(ILoginParameters loginParameters)
		{
			if (!_loginActions.LoginParametersAreValid(loginParameters))
				return;

			IAccountLoginData loginData;
			try
			{
				//todo: return just the login reply, character data should be put into repository
				loginData = await _loginActions.LoginToServer(loginParameters);
			}
			catch (EmptyPacketReceivedException)
			{
				//todo: make sure disconnect mechanics are handled correctly here
				_errorDisplayAction.ShowError(ConnectResult.SocketError);
				_gameStateActions.ChangeToState(GameStates.Initial);
				return;
			}
			catch (NoDataSentException)
			{
				_errorDisplayAction.ShowError(ConnectResult.SocketError);
				_gameStateActions.ChangeToState(GameStates.Initial);
				return;
			}
			catch (MalformedPacketException)
			{
				_errorDisplayAction.ShowError(ConnectResult.SocketError);
				_gameStateActions.ChangeToState(GameStates.Initial);
				return;
			}

			if (loginData.Response == LoginReply.Ok)
			{
				_gameStateActions.ChangeToState(GameStates.LoggedIn);
			}
			else
			{
				//show correct login response dialog
			}
		}

		public async Task LoginToCharacter()
		{
			throw new System.NotImplementedException();
		}
	}
}