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
        private readonly IChatBubbleActions _chatBubbleActions;
        private readonly IHudControlProvider _hudControlProvider;

        public ChatController(IChatTextBoxActions chatTextBoxActions,
                              IChatActions chatActions,
                              IPrivateMessageActions privateMessageActions,
                              IChatBubbleActions chatBubbleActions,
                              IHudControlProvider hudControlProvider)
        {
            _chatTextBoxActions = chatTextBoxActions;
            _chatActions = chatActions;
            _privateMessageActions = privateMessageActions;
            _chatBubbleActions = chatBubbleActions;
            _hudControlProvider = hudControlProvider;
        }

        public void SendChatAndClearTextBox()
        {
            var localTypedText = ChatTextBox.Text;
            var targetCharacter = _privateMessageActions.GetTargetCharacter(localTypedText);

            var updatedChat = _chatActions.SendChatToServer(localTypedText, targetCharacter);

            _chatTextBoxActions.ClearChatText();

            _chatBubbleActions.ShowChatBubbleForMainCharacter(updatedChat);
        }

        public void SelectChatTextBox()
        {
            _chatTextBoxActions.FocusChatTextBox();
        }

        private ChatTextBox ChatTextBox => _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox);
    }

    public interface IChatController
    {
        void SendChatAndClearTextBox();

        void SelectChatTextBox();
    }
}
