// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Input;
using EndlessClient.UIControls;

namespace EndlessClient.Controllers
{
    public class ChatController : IChatController
    {
        private readonly IKeyboardDispatcherProvider _keyboardDispatcherProvider;
        private readonly IHudControlProvider _hudControlProvider;

        public ChatController(IKeyboardDispatcherProvider keyboardDispatcherProvider,
                              IHudControlProvider hudControlProvider)
        {
            _keyboardDispatcherProvider = keyboardDispatcherProvider;
            _hudControlProvider = hudControlProvider;
        }

        public void SendChatAndClearTextBox()
        {
        }

        public void SelectChatTextBox()
        {
            //todo: put this into actions?
            if (_keyboardDispatcherProvider.Dispatcher.Subscriber != null)
                _keyboardDispatcherProvider.Dispatcher.Subscriber.Selected = false;

            var chatTextBox = _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox);
            _keyboardDispatcherProvider.Dispatcher.Subscriber = chatTextBox;
            chatTextBox.Selected = true;
        }

        public void UpdateChatModeGraphic()
        {
        }
    }
}
