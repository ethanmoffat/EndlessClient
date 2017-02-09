// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Input;
using EndlessClient.UIControls;
using XNAControls;

namespace EndlessClient.HUD.Chat
{
    public class ChatTextBoxActions : IChatTextBoxActions
    {
        private readonly IKeyboardDispatcherProvider _keyboardDispatcherProvider;
        private readonly IHudControlProvider _hudControlProvider;

        public ChatTextBoxActions(IKeyboardDispatcherProvider keyboardDispatcherProvider,
                                  IHudControlProvider hudControlProvider)
        {
            _keyboardDispatcherProvider = keyboardDispatcherProvider;
            _hudControlProvider = hudControlProvider;
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

        private KeyboardDispatcher KeyboardDispatcher => _keyboardDispatcherProvider.Dispatcher;

        private ChatTextBox GetChatTextBox()
        {
            return _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox);
        }
    }
}