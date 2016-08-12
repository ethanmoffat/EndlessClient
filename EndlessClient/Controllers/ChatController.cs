// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.HUD.Chat;

namespace EndlessClient.Controllers
{
    public class ChatController : IChatController
    {
        private readonly IChatTextBoxActions _chatTextBoxActions;
        private readonly IChatModeGraphicActions _chatModeGraphicActions;

        public ChatController(IChatTextBoxActions chatTextBoxActions,
                              IChatModeGraphicActions chatModeGraphicActions)
        {
            _chatTextBoxActions = chatTextBoxActions;
            _chatModeGraphicActions = chatModeGraphicActions;
        }

        public void SendChatAndClearTextBox()
        {
            //todo: send chat string to server (see HUD._doTalk)
            _chatTextBoxActions.ClearChatText();
        }

        public void SelectChatTextBox()
        {
            _chatTextBoxActions.FocusChatTextBox();
        }

        public void UpdateChatModeGraphic()
        {
            _chatModeGraphicActions.UpdateChatMode();
        }
    }
}
