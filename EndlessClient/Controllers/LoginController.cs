// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.IO;
using System.Threading.Tasks;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Actions;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.IO;
using EOLib.IO.Actions;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EndlessClient.Controllers
{
	public class LoginController : ILoginController
	{
		private readonly ILoginActions _loginActions;
		private readonly IFileLoadActions _fileLoadActions;
		private readonly IFileRequestActions _fileRequestActions;
		private readonly IGameStateActions _gameStateActions;
		private readonly IErrorDialogDisplayAction _errorDisplayAction;
		private readonly ISafeInBandNetworkOperationFactory _networkOperationFactory;
		private readonly IGameLoadingDialogFactory _gameLoadingDialogFactory;
		private readonly ICurrentMapStateProvider _currentMapStateProvider;

		public LoginController(ILoginActions loginActions,
							   IFileLoadActions fileLoadActions,
							   IFileRequestActions fileRequestActions,
							   IGameStateActions gameStateActions,
							   IErrorDialogDisplayAction errorDisplayAction,
							   ISafeInBandNetworkOperationFactory networkOperationFactory,
							   IGameLoadingDialogFactory gameLoadingDialogFactory,
							   ICurrentMapStateProvider currentMapStateProvider)
		{
			_loginActions = loginActions;
			_fileLoadActions = fileLoadActions;
			_fileRequestActions = fileRequestActions;
			_gameStateActions = gameStateActions;
			_errorDisplayAction = errorDisplayAction;
			_networkOperationFactory = networkOperationFactory;
			_gameLoadingDialogFactory = gameLoadingDialogFactory;
			_currentMapStateProvider = currentMapStateProvider;
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

			try
			{
				_fileLoadActions.LoadMapFileByID(_currentMapStateProvider.CurrentMapID);
			}
			catch (IOException)
			{
				// Try to load the map now that we know what Map ID we need
				// non-fatal exception
			}

			GameLoadingDialog gameLoadingDialog = null;
			try
			{
				gameLoadingDialog = _gameLoadingDialogFactory.CreateGameLoadingDialog();

				await InitialDelayInReleaseMode();

				if (_fileRequestActions.NeedsFile(InitFileType.Map, _currentMapStateProvider.CurrentMapID))
				{
					gameLoadingDialog.SetState(GameLoadingDialogState.Map);
					if (!await SafeGetFile(async () => await _fileRequestActions.GetMapFromServer(_currentMapStateProvider.CurrentMapID)))
						return;
					await Task.Delay(1000);
				}

				if (_fileRequestActions.NeedsFile(InitFileType.Item))
				{
					gameLoadingDialog.SetState(GameLoadingDialogState.Item);
					if (!await SafeGetFile(_fileRequestActions.GetItemFileFromServer))
						return;
					await Task.Delay(1000);
				}

				if (_fileRequestActions.NeedsFile(InitFileType.Npc))
				{
					gameLoadingDialog.SetState(GameLoadingDialogState.NPC);
					if (!await SafeGetFile(_fileRequestActions.GetNPCFileFromServer))
						return;
					await Task.Delay(1000);
				}

				if (_fileRequestActions.NeedsFile(InitFileType.Spell))
				{
					gameLoadingDialog.SetState(GameLoadingDialogState.Spell);
					if (!await SafeGetFile(_fileRequestActions.GetSpellFileFromServer))
						return;
					await Task.Delay(1000);
				}

				if (_fileRequestActions.NeedsFile(InitFileType.Class))
				{
					gameLoadingDialog.SetState(GameLoadingDialogState.Class);
					if (!await SafeGetFile(_fileRequestActions.GetClassFileFromServer))
						return;
					await Task.Delay(1000);
				}

				gameLoadingDialog.SetState(GameLoadingDialogState.LoadingGame);

				var completeCharacterLoginOperation = _networkOperationFactory.CreateSafeOperation(
					_loginActions.CompleteCharacterLogin,
					SetInitialStateAndShowError,
					SetInitialStateAndShowError);
				if (!await completeCharacterLoginOperation.Invoke())
					return;

				await Task.Delay(1000); //always wait 1 second

				_gameStateActions.ChangeToState(GameStates.PlayingTheGame);
			}
			finally
			{
				if (gameLoadingDialog != null)
					gameLoadingDialog.Close();
			}
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

		private async Task InitialDelayInReleaseMode()
		{
#if DEBUG
			await Task.FromResult(false); //no-op in debug
#else
			await Task.Delay(5000);
#endif
		}

		private async Task<bool> SafeGetFile(Func<Task> operation)
		{
			var op = _networkOperationFactory.CreateSafeOperation(
						operation,
						SetInitialStateAndShowError,
						SetInitialStateAndShowError);
			return await op.Invoke();
		}
	}
}