// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Chat;
using EndlessClient.HUD.Controls;
using EndlessClient.UIControls;
using EOLib.Domain.Chat;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EndlessClient.Controllers
{
    public class ChatController : IChatController
    {
        private readonly IChatTextBoxActions _chatTextBoxActions;
        private readonly IChatActions _chatActions;
        private readonly IPrivateMessageActions _privateMessageActions;
        private readonly IGameStateActions _gameStateActions;
        private readonly IErrorDialogDisplayAction _errorDisplayAction;
        private readonly IChatBubbleActions _chatBubbleActions;
        private readonly ISafeNetworkOperationFactory _safeNetworkOperationFactory;
        private readonly IHudControlProvider _hudControlProvider;

        public ChatController(IChatTextBoxActions chatTextBoxActions,
                              IChatActions chatActions,
                              IPrivateMessageActions privateMessageActions,
                              IGameStateActions gameStateActions,
                              IErrorDialogDisplayAction errorDisplayAction,
                              IChatBubbleActions chatBubbleActions,
                              ISafeNetworkOperationFactory safeNetworkOperationFactory,
                              IHudControlProvider hudControlProvider)
        {
            _chatTextBoxActions = chatTextBoxActions;
            _chatActions = chatActions;
            _privateMessageActions = privateMessageActions;
            _gameStateActions = gameStateActions;
            _errorDisplayAction = errorDisplayAction;
            _chatBubbleActions = chatBubbleActions;
            _safeNetworkOperationFactory = safeNetworkOperationFactory;
            _hudControlProvider = hudControlProvider;
        }

        public async Task SendChatAndClearTextBox()
        {
            var targetCharacter = _privateMessageActions.GetTargetCharacter(ChatTextBox.Text);
            var sendChatOperation = _safeNetworkOperationFactory.CreateSafeAsyncOperation(
                async () => await _chatActions.SendChatToServer(ChatTextBox.Text, targetCharacter),
                SetInitialStateAndShowError);

            if (!await sendChatOperation.Invoke())
                return;

            _chatTextBoxActions.ClearChatText();

            _chatBubbleActions.ShowChatBubbleForMainCharacter();
        }

        public void SelectChatTextBox()
        {
            _chatTextBoxActions.FocusChatTextBox();
        }

        private void SetInitialStateAndShowError(NoDataSentException ex)
        {
            _gameStateActions.ChangeToState(GameStates.Initial);
            _errorDisplayAction.ShowException(ex);
        }

        private ChatTextBox ChatTextBox => _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox);
    }

    public interface IChatController
    {
        Task SendChatAndClearTextBox();

        void SelectChatTextBox();
    }
}
