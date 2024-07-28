using AutomaticTypeMapper;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EOLib.Domain.Account;
using EOLib.Net;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Threading.Tasks;
using XNAControls;

namespace EndlessClient.Controllers;

[MappedType(BaseType = typeof(IAccountController))]
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
            () => _accountActions.CheckAccountNameWithServer(createAccountParameters.AccountName),
            SetInitialStateAndShowError,
            SetInitialStateAndShowError);
        if (!await checkNameOperation.Invoke().ConfigureAwait(false))
            return;

        var nameResult = checkNameOperation.Result;
        if (nameResult < (AccountReply)10)
        {
            _accountDialogDisplayActions.ShowCreateAccountServerError(nameResult);
            return;
        }

        var result = await _accountDialogDisplayActions.ShowCreatePendingDialog().ConfigureAwait(false);
        if (result == XNADialogResult.Cancel)
            return;

        var createAccountOperation = _networkOperationFactory.CreateSafeBlockingOperation(
            () => _accountActions.CreateAccount(createAccountParameters, (int)nameResult),
            SetInitialStateAndShowError,
            SetInitialStateAndShowError);
        if (!await createAccountOperation.Invoke().ConfigureAwait(false))
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
        var changePasswordResult = await _accountDialogDisplayActions.ShowChangePasswordDialog().ConfigureAwait(false);
        changePasswordResult.MatchSome(changePasswordParameters =>
            {
                var changePasswordOperation = _networkOperationFactory.CreateSafeBlockingOperation(
                    () => _accountActions.ChangePassword(changePasswordParameters),
                    SetInitialStateAndShowError,
                    SetInitialStateAndShowError);

                var opTask = changePasswordOperation.Invoke();
                opTask.ContinueWith(t =>
                {
                    if (t.Result)
                    {
                        var result = changePasswordOperation.Result;
                        _accountDialogDisplayActions.ShowCreateAccountServerError(result);
                    }
                });
            });
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

public interface IAccountController
{
    Task CreateAccount(ICreateAccountParameters createAccountParameters);

    Task ChangePassword();
}