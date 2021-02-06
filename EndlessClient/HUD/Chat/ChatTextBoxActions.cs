using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Input;
using EndlessClient.UIControls;
using XNAControls;

namespace EndlessClient.HUD.Chat
{
    [MappedType(BaseType = typeof(IChatTextBoxActions))]
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
            // TODO: re-enable with UI rework
            //var chatTextBox = GetChatTextBox();
            //chatTextBox.Text = "";
        }

        public void FocusChatTextBox()
        {
            // TODO: re-enable with UI rework
            //if (KeyboardDispatcher.Subscriber != null)
            //    KeyboardDispatcher.Subscriber.Selected = false;

            //KeyboardDispatcher.Subscriber = GetChatTextBox();
            //KeyboardDispatcher.Subscriber.Selected = true;
        }

        private KeyboardDispatcher KeyboardDispatcher => _keyboardDispatcherProvider.Dispatcher;

        private ChatTextBox GetChatTextBox()
        {
            return _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox);
        }
    }
}