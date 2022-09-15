using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.ControlSets;
using EndlessClient.HUD;
using EndlessClient.HUD.Chat;
using EndlessClient.HUD.Controls;
using EndlessClient.UIControls;
using EOLib.Domain.Chat;
using EOLib.Domain.Chat.Commands;
using EOLib.Localization;
using System;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class ChatController : IChatController
    {
        private readonly IChatTextBoxActions _chatTextBoxActions;
        private readonly IChatActions _chatActions;
        private readonly IPrivateMessageActions _privateMessageActions;
        private readonly IChatBubbleActions _chatBubbleActions;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ISfxPlayer _sfxPlayer;

        public ChatController(IChatTextBoxActions chatTextBoxActions,
                              IChatActions chatActions,
                              IPrivateMessageActions privateMessageActions,
                              IChatBubbleActions chatBubbleActions,
                              IStatusLabelSetter statusLabelSetter,
                              IHudControlProvider hudControlProvider,
                              ISfxPlayer sfxPlayer)
        {
            _chatTextBoxActions = chatTextBoxActions;
            _chatActions = chatActions;
            _privateMessageActions = privateMessageActions;
            _chatBubbleActions = chatBubbleActions;
            _statusLabelSetter = statusLabelSetter;
            _hudControlProvider = hudControlProvider;
            _sfxPlayer = sfxPlayer;
        }

        public void SendChatAndClearTextBox()
        {
            var localTypedText = ChatTextBox.Text;
            var (pmCheckOk, targetCharacter) = _privateMessageActions.GetTargetCharacter(localTypedText);

            if (pmCheckOk)
            {
                if (!string.IsNullOrEmpty(targetCharacter))
                {
                    _sfxPlayer.PlaySfx(SoundEffectID.PrivateMessageSent);
                }

                var (result, updatedChat) = _chatActions.SendChatToServer(localTypedText, targetCharacter);
                switch (result)
                {
                    case ChatResult.Ok: _chatBubbleActions.ShowChatBubbleForMainCharacter(updatedChat); break;
                    case ChatResult.YourMindPrevents: _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.YOUR_MIND_PREVENTS_YOU_TO_SAY); break;
                    case ChatResult.Command:
                        {
                            var commandText = updatedChat[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)[0];
                            _sfxPlayer.PlaySfx(commandText switch
                            {
                                NoWallCommand.Text or PingCommand.Text => SoundEffectID.ServerCommand,
                                _ => SoundEffectID.ServerMessage,
                            });
                        }
                        break;
                    case ChatResult.AdminAnnounce: _sfxPlayer.PlaySfx(SoundEffectID.AdminAnnounceReceived); goto case ChatResult.Ok;
                    case ChatResult.HideSpeechBubble: break; // no-op
                    case ChatResult.HideAll: break; // no-op
                }
            }

            _chatTextBoxActions.ClearChatText();
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
