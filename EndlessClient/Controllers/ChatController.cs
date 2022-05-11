using System.Threading.Tasks;
using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EndlessClient.HUD;
using EndlessClient.HUD.Chat;
using EndlessClient.HUD.Controls;
using EndlessClient.UIControls;
using EOLib.Domain.Chat;
using EOLib.Localization;
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
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IHudControlProvider _hudControlProvider;

        public ChatController(IChatTextBoxActions chatTextBoxActions,
                              IChatActions chatActions,
                              IPrivateMessageActions privateMessageActions,
                              IChatBubbleActions chatBubbleActions,
                              IStatusLabelSetter statusLabelSetter,
                              IHudControlProvider hudControlProvider)
        {
            _chatTextBoxActions = chatTextBoxActions;
            _chatActions = chatActions;
            _privateMessageActions = privateMessageActions;
            _chatBubbleActions = chatBubbleActions;
            _statusLabelSetter = statusLabelSetter;
            _hudControlProvider = hudControlProvider;
        }

        public void SendChatAndClearTextBox()
        {
            var localTypedText = ChatTextBox.Text;
            var targetCharacter = _privateMessageActions.GetTargetCharacter(localTypedText);

            var (ok, updatedChat) = _chatActions.SendChatToServer(localTypedText, targetCharacter);

            _chatTextBoxActions.ClearChatText();

            if (ok)
            {
                _chatBubbleActions.ShowChatBubbleForMainCharacter(updatedChat);
            }
            else
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.YOUR_MIND_PREVENTS_YOU_TO_SAY);
            }
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
