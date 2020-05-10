using System.Threading.Tasks;
using AutomaticTypeMapper;
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
    [MappedType(BaseType = typeof(IChatController))]
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
            var localTypedText = ChatTextBox.Text;
            var targetCharacter = _privateMessageActions.GetTargetCharacter(localTypedText);
            var sendChatOperation = _safeNetworkOperationFactory.CreateSafeAsyncOperation(
                async () => await _chatActions.SendChatToServer(localTypedText, targetCharacter),
                SetInitialStateAndShowError);

            if (!await sendChatOperation.Invoke())
                return;

            _chatTextBoxActions.ClearChatText();

            _chatBubbleActions.ShowChatBubbleForMainCharacter(localTypedText);
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
