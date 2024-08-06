using System;
using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Localization;

namespace EndlessClient.HUD.Chat
{
    [MappedType(BaseType = typeof(IPrivateMessageActions))]
    public class PrivateMessageActions : IPrivateMessageActions
    {
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IChatProvider _chatProvider;
        private readonly IChatTypeCalculator _chatTypeCalculator;
        private readonly IStatusLabelSetter _statusLabelSetter;

        public PrivateMessageActions(IHudControlProvider hudControlProvider,
                                     ICharacterProvider characterProvider,
                                     IChatProvider chatProvider,
                                     IChatTypeCalculator chatTypeCalculator,
                                     IStatusLabelSetter statusLabelSetter)
        {
            _hudControlProvider = hudControlProvider;
            _characterProvider = characterProvider;
            _chatProvider = chatProvider;
            _chatTypeCalculator = chatTypeCalculator;
            _statusLabelSetter = statusLabelSetter;
        }

        public (bool, string) GetTargetCharacter(string localTypedText)
        {
            if (_chatTypeCalculator.CalculateChatType(localTypedText) != ChatType.PM)
            {
                return (true, string.Empty);
            }

            if (localTypedText.Length < 2 || localTypedText[1] == ' ')
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_INVALID_INPUT_TRY);
                return (false, string.Empty);
            }

            if (CurrentTab == ChatTab.Private1)
            {
                return (true, _chatProvider.PMTarget1);
            }
            else if (CurrentTab == ChatTab.Private2)
            {
                return (true, _chatProvider.PMTarget2);
            }

            var messageParts = localTypedText[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (messageParts.Length <= 1)
            {
                _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_INVALID_INPUT_TRY);
                return (false, string.Empty);
            }

            if (string.Equals(messageParts[0], _characterProvider.MainCharacter.Name, StringComparison.OrdinalIgnoreCase))
                return (false, string.Empty);

            ChatPanel.TryStartNewPrivateChat(messageParts[0]);
            return (true, messageParts[0]);
        }

        private ChatPanel ChatPanel => _hudControlProvider.GetComponent<ChatPanel>(HudControlIdentifier.ChatPanel);

        private ChatTab CurrentTab => ChatPanel.CurrentTab;
    }

    public interface IPrivateMessageActions
    {
        (bool Ok, string TargetCharacter) GetTargetCharacter(string localTypedText);
    }
}
