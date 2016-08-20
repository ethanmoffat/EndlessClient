// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Threading.Tasks;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Domain.Account;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EndlessClient.Controllers
{
    public class AccountController : IAccountController
    {
        private readonly IAccountDialogDisplayActions _accountDialogDisplayActions;
        private readonly IErrorDialogDisplayAction _errorDisplayAction;
        private readonly IAccountActions _accountActions;
        private readonly IGameStateActions _gameStateActions;
        private readonly ISafeNetworkOperationFactory _networkOperationFactory;

        public AccountController(IAccountDialogDisplayActions accountDialogDisplayActions,
                                 IErrorDialogDisplayAction errorDisplayAction,
                                 IAccountActions accountActions,
                                 IGameStateActions gameStateActions,
                                 ISafeNetworkOperationFactory networkOperationFactory)
        {
            _accountDialogDisplayActions = accountDialogDisplayActions;
            _errorDisplayAction = errorDisplayAction;
            _accountActions = accountActions;
            _gameStateActions = gameStateActions;
            _networkOperationFactory = networkOperationFactory;
        }

        public async Task CreateAccount(ICreateAccountParameters createAccountParameters)
        {
            var paramsValidationResult = _accountActions.CheckAccountCreateParameters(createAccountParameters);
            if (paramsValidationResult.FaultingParameter != WhichParameter.None)
            {
                _accountDialogDisplayActions.ShowCreateParameterValidationError(paramsValidationResult);
                return;
            }

            var checkNameOperation = _networkOperationFactory.CreateSafeBlockingOperation(
                async () => await _accountActions.CheckAccountNameWithServer(createAccountParameters.AccountName),
                SetInitialStateAndShowError,
                SetInitialStateAndShowError);
            if (!await checkNameOperation.Invoke())
                return;

            var nameResult = checkNameOperation.Result;
            if (nameResult != AccountReply.Continue)
            {
                _accountDialogDisplayActions.ShowCreateAccountServerError(nameResult);
                return;
            }

            if (!await ShowAccountCreationPendingDialog()) return;

            var createAccountOperation = _networkOperationFactory.CreateSafeBlockingOperation(
                async () => await _accountActions.CreateAccount(createAccountParameters),
                SetInitialStateAndShowError,
                SetInitialStateAndShowError);
            if (!await createAccountOperation.Invoke())
                return;

            var accountResult = createAccountOperation.Result;
            if (accountResult != AccountReply.Created)
            {
                _accountDialogDisplayActions.ShowCreateAccountServerError(accountResult);
                return;
            }

            _gameStateActions.ChangeToState(GameStates.Initial);
            _accountDialogDisplayActions.ShowSuccessMessage();
        }

        public async Task ChangePassword()
        {
            IChangePasswordParameters changePasswordParameters;
            try
            {
                changePasswordParameters = await _accountDialogDisplayActions.ShowChangePasswordDialog();
            }
            catch (OperationCanceledException) { return; }

            var changePasswordOperation = _networkOperationFactory.CreateSafeBlockingOperation(
                async () => await _accountActions.ChangePassword(changePasswordParameters),
                SetInitialStateAndShowError,
                SetInitialStateAndShowError);
            if (!await changePasswordOperation.Invoke())
                return;

            var result = changePasswordOperation.Result;
            _accountDialogDisplayActions.ShowCreateAccountServerError(result);
        }

        private async Task<bool> ShowAccountCreationPendingDialog()
        {
            try
            {
                await _accountDialogDisplayActions.ShowCreatePendingDialog();
            }
            catch (OperationCanceledException) { return false; }

            return true;
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