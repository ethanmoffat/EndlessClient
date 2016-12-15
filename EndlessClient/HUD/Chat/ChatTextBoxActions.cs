// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Input;
using EndlessClient.UIControls;
using EOLib.Domain.Chat;
using XNAControls;

namespace EndlessClient.HUD.Chat
{
    public class ChatTextBoxActions : IChatTextBoxActions
    {
        private readonly IKeyboardDispatcherProvider _keyboardDispatcherProvider;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly IChatRepository _chatRepository;

        public ChatTextBoxActions(IKeyboardDispatcherProvider keyboardDispatcherProvider,
                                  IHudControlProvider hudControlProvider,
                                  IChatRepository chatRepository)
        {
            _keyboardDispatcherProvider = keyboardDispatcherProvider;
            _hudControlProvider = hudControlProvider;
            _chatRepository = chatRepository;
        }

        public void ClearChatText()
        {
            var chatTextBox = GetChatTextBox();
            chatTextBox.Text = "";
        }

        public void FocusChatTextBox()
        {
            if (KeyboardDispatcher.Subscriber != null)
                KeyboardDispatcher.Subscriber.Selected = false;

            KeyboardDispatcher.Subscriber = GetChatTextBox();
            KeyboardDispatcher.Subscriber.Selected = true;
        }

        public void UpdateChatTextRepository()
        {
            var chatTextBox = GetChatTextBox();
            _chatRepository.LocalTypedText = chatTextBox.Text;
        }

        private KeyboardDispatcher KeyboardDispatcher
        {
            get { return _keyboardDispatcherProvider.Dispatcher; }
        }

        private ChatTextBox GetChatTextBox()
        {
            return _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox);
        }
    }
}