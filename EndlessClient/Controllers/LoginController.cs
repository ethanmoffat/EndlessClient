using System;
using System.IO;
using System.Threading.Tasks;
using AutomaticTypeMapper;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Actions;
using EndlessClient.Dialogs.Factories;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EndlessClient.HUD.Chat;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Protocol;
using EOLib.IO.Actions;
using EOLib.Localization;
using EOLib.Net;
using EOLib.Net.Communication;
using EOLib.Net.FileTransfer;

namespace EndlessClient.Controllers
{
    [MappedType(BaseType = typeof(ILoginController))]
    public class LoginController : ILoginController
    {
        private readonly ILoginActions _loginActions;
        private readonly IMapFileLoadActions _mapFileLoadActions;
        private readonly IFileRequestActions _fileRequestActions;
        private readonly IGameStateActions _gameStateActions;
        private readonly IChatTextBoxActions _chatTextBoxActions;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IChatRepository _chatRepository;
        private readonly INewsProvider _newsProvider;
        private readonly IErrorDialogDisplayAction _errorDisplayAction;
        private readonly ISafeNetworkOperationFactory _networkOperationFactory;
        private readonly IGameLoadingDialogFactory _gameLoadingDialogFactory;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly IFirstTimePlayerActions _firstTimePlayerActions;
        private readonly IMapChangedActions _mapChangedActions;

        public LoginController(ILoginActions loginActions,
                               IMapFileLoadActions mapFileLoadActions,
                               IFileRequestActions fileRequestActions,
                               IGameStateActions gameStateActions,
                               IChatTextBoxActions chatTextBoxActions,
                               IErrorDialogDisplayAction errorDisplayAction,
                               IFirstTimePlayerActions firstTimePlayerActions,
                               IMapChangedActions mapChangedActions,
                               ISafeNetworkOperationFactory networkOperationFactory,
                               IGameLoadingDialogFactory gameLoadingDialogFactory,
                               ICurrentMapStateProvider currentMapStateProvider,
                               IStatusLabelSetter statusLabelSetter,
                               ILocalizedStringFinder localizedStringFinder,
                               IChatRepository chatRepository,
                               INewsProvider newsProvider)
        {
            _loginActions = loginActions;
            _mapFileLoadActions = mapFileLoadActions;
            _fileRequestActions = fileRequestActions;
            _gameStateActions = gameStateActions;
            _chatTextBoxActions = chatTextBoxActions;
            _errorDisplayAction = errorDisplayAction;
            _firstTimePlayerActions = firstTimePlayerActions;
            _mapChangedActions = mapChangedActions;
            _networkOperationFactory = networkOperationFactory;
            _gameLoadingDialogFactory = gameLoadingDialogFactory;
            _currentMapStateProvider = currentMapStateProvider;
            _statusLabelSetter = statusLabelSetter;
            _localizedStringFinder = localizedStringFinder;
            _chatRepository = chatRepository;
            _newsProvider = newsProvider;
        }

        public async Task LoginToAccount(ILoginParameters loginParameters)
        {
            if (!_loginActions.LoginParametersAreValid(loginParameters))
                return;

            var loginToServerOperation = _networkOperationFactory.CreateSafeBlockingOperation(
                async () => await _loginActions.LoginToServer(loginParameters),
                SetInitialStateAndShowError, SetInitialStateAndShowError);

            if (!await loginToServerOperation.Invoke())
                return;
            var reply = loginToServerOperation.Result;

            if (reply == LoginReply.Ok)
                _gameStateActions.ChangeToState(GameStates.LoggedIn);
            else
            {
                _errorDisplayAction.ShowLoginError(reply);
                _gameStateActions.ChangeToState(GameStates.Initial);
            }
        }

        public async Task LoginToCharacter(ICharacter character)
        {
            var requestCharacterLoginOperation = _networkOperationFactory.CreateSafeBlockingOperation(
                async () => await _loginActions.RequestCharacterLogin(character),
                SetInitialStateAndShowError, SetInitialStateAndShowError);
            if (!await requestCharacterLoginOperation.Invoke())
                return;

            var unableToLoadMap = false;
            try
            {
                _mapFileLoadActions.LoadMapFileByID(_currentMapStateProvider.CurrentMapID);
            }
            catch (IOException)
            {
                // Try to load the map now that we know what Map ID we need
                // non-fatal exception
                unableToLoadMap = true;
            }

            GameLoadingDialog gameLoadingDialog = null;
            try
            {
                gameLoadingDialog = _gameLoadingDialogFactory.CreateGameLoadingDialog();
                gameLoadingDialog.ShowDialog();

                await InitialDelayInReleaseMode();

                if (unableToLoadMap || _fileRequestActions.NeedsFileForLogin(InitFileType.Map, _currentMapStateProvider.CurrentMapID))
                {
                    gameLoadingDialog.SetState(GameLoadingDialogState.Map);
                    if (!await SafeGetFile(async () => await _fileRequestActions.GetMapFromServer(_currentMapStateProvider.CurrentMapID)))
                        return;
                    await Task.Delay(1000);
                }

                if (_fileRequestActions.NeedsFileForLogin(InitFileType.Item))
                {
                    gameLoadingDialog.SetState(GameLoadingDialogState.Item);
                    if (!await SafeGetFile(_fileRequestActions.GetItemFileFromServer))
                        return;
                    await Task.Delay(1000);
                }

                if (_fileRequestActions.NeedsFileForLogin(InitFileType.Npc))
                {
                    gameLoadingDialog.SetState(GameLoadingDialogState.NPC);
                    if (!await SafeGetFile(_fileRequestActions.GetNPCFileFromServer))
                        return;
                    await Task.Delay(1000);
                }

                if (_fileRequestActions.NeedsFileForLogin(InitFileType.Spell))
                {
                    gameLoadingDialog.SetState(GameLoadingDialogState.Spell);
                    if (!await SafeGetFile(_fileRequestActions.GetSpellFileFromServer))
                        return;
                    await Task.Delay(1000);
                }

                if (_fileRequestActions.NeedsFileForLogin(InitFileType.Class))
                {
                    gameLoadingDialog.SetState(GameLoadingDialogState.Class);
                    if (!await SafeGetFile(_fileRequestActions.GetClassFileFromServer))
                        return;
                    await Task.Delay(1000);
                }

                gameLoadingDialog.SetState(GameLoadingDialogState.LoadingGame);

                var completeCharacterLoginOperation = _networkOperationFactory.CreateSafeBlockingOperation(
                    _loginActions.CompleteCharacterLogin,
                    SetInitialStateAndShowError,
                    SetInitialStateAndShowError);
                if (!await completeCharacterLoginOperation.Invoke())
                    return;

                if (completeCharacterLoginOperation.Result == CharacterLoginReply.RequestDenied)
                {
                    // https://discord.com/channels/723989119503696013/787685796055482368/946634672295784509
                    // Sausage: 'I have WELCOME_REPLY 3 as returning a "server is busy" message if you send it and then disconnect the client'
                    _gameStateActions.ChangeToState(GameStates.Initial);
                    _errorDisplayAction.ShowLoginError(LoginReply.Busy);
                    return;
                }

                ClearChat();
                AddDefaultTextToChat();

                await Task.Delay(1000); //always wait 1 second
            }
            finally
            {
                gameLoadingDialog?.CloseDialog();
            }

            _gameStateActions.ChangeToState(GameStates.PlayingTheGame);
            _chatTextBoxActions.FocusChatTextBox();
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING,
                                              EOResourceID.LOADING_GAME_HINT_FIRST);
            _firstTimePlayerActions.WarnFirstTimePlayers();
            _mapChangedActions.ActiveCharacterEnterMapForLogin();
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

        private Task InitialDelayInReleaseMode()
        {
#if DEBUG
            return Task.Delay(2000);
#else
            return Task.Delay(5000);
#endif
        }

        private async Task<bool> SafeGetFile(Func<Task> operation)
        {
            var op = _networkOperationFactory.CreateSafeBlockingOperation(
                        operation,
                        SetInitialStateAndShowError,
                        SetInitialStateAndShowError);
            return await op.Invoke();
        }

        private void ClearChat()
        {
            foreach(var chat in _chatRepository.AllChat.Values)
            {
                chat.Clear();
            }
        }

        private void AddDefaultTextToChat()
        {
            var server = _localizedStringFinder.GetString(EOResourceID.STRING_SERVER);
            var serverMessage1 = _localizedStringFinder.GetString(EOResourceID.GLOBAL_CHAT_SERVER_MESSAGE_1);
            var serverMessage2 = _localizedStringFinder.GetString(EOResourceID.GLOBAL_CHAT_SERVER_MESSAGE_2);

            _chatRepository.AllChat[ChatTab.Local].Add(
                new ChatData(server, _newsProvider.NewsHeader, ChatIcon.Note, ChatColor.Server));
            _chatRepository.AllChat[ChatTab.Global].Add(
                new ChatData(server, serverMessage1, ChatIcon.Note, ChatColor.Server));
            _chatRepository.AllChat[ChatTab.Global].Add(
                new ChatData(server, serverMessage2, ChatIcon.Note, ChatColor.Server));
        }
    }

    public interface ILoginController
    {
        Task LoginToAccount(ILoginParameters loginParameters);

        Task LoginToCharacter(ICharacter character);
    }
}