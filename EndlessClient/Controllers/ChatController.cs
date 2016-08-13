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
        private readonly IChatModeGraphicActions _chatModeGraphicActions;
        private readonly IChatTextProvider _chatTextRepository;

        public ChatController(IChatTextBoxActions chatTextBoxActions,
                              IChatModeGraphicActions chatModeGraphicActions,
                              IChatTextProvider chatTextRepository)
        {
            _chatTextBoxActions = chatTextBoxActions;
            _chatModeGraphicActions = chatModeGraphicActions;
            _chatTextRepository = chatTextRepository;
        }

        public void SendChatAndClearTextBox()
        {
            //todo: send chat string to server (see HUD._doTalk)
            _chatTextBoxActions.ClearChatText();
            _chatTextBoxActions.UpdateChatTextRepository();
            _chatModeGraphicActions.UpdateChatMode();
        }

        public void SelectChatTextBox()
        {
            _chatTextBoxActions.FocusChatTextBox();
        }

        public void ChatTextChanged()
        {
            _chatTextBoxActions.UpdateChatTextRepository();

            if (SingleCharacterEnteredOrDeleted)
                _chatModeGraphicActions.UpdateChatMode();
        }

        private bool SingleCharacterEnteredOrDeleted
        {
            get
            {
                return (_chatTextRepository.ChatText.Length == 0 && _chatTextRepository.PreviousText.Length == 1) ||
                       (_chatTextRepository.ChatText.Length == 1 && _chatTextRepository.PreviousText.Length == 0);
            }
        }
    }
}
