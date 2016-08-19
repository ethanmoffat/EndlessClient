// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.HUD.Chat;
using EOLib.Domain.Chat;

namespace EndlessClient.Controllers
{
    public class ChatController : IChatController
    {
        private readonly IChatTextBoxActions _chatTextBoxActions;
        private readonly IChatActions _chatActions;

        public ChatController(IChatTextBoxActions chatTextBoxActions,
                              IChatActions chatActions)
        {
            _chatTextBoxActions = chatTextBoxActions;
            _chatActions = chatActions;
        }

        public void SendChatAndClearTextBox()
        {
            _chatActions.SendChatToServer();
            _chatTextBoxActions.ClearChatText();
            _chatTextBoxActions.UpdateChatTextRepository();
        }

        public void SelectChatTextBox()
        {
            _chatTextBoxActions.FocusChatTextBox();
        }

        public void ChatTextChanged()
        {
            _chatTextBoxActions.UpdateChatTextRepository();
        }
    }
}
