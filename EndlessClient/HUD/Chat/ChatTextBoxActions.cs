using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.UIControls;

namespace EndlessClient.HUD.Chat
{
    [AutoMappedType]
    public class ChatTextBoxActions : IChatTextBoxActions
    {
        private readonly IHudControlProvider _hudControlProvider;

        public ChatTextBoxActions(IHudControlProvider hudControlProvider)
        {
            _hudControlProvider = hudControlProvider;
        }

        public void ClearChatText()
        {
            var chatTextBox = GetChatTextBox();
            chatTextBox.Text = "";
        }

        public void FocusChatTextBox()
        {
            GetChatTextBox().Selected = true;
        }

        private ChatTextBox GetChatTextBox()
        {
            return _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox);
        }
    }
}