using System;
using AutomaticTypeMapper;
using EOLib.Domain.Character;

namespace EOLib.Domain.Chat
{
    [AutoMappedType]
    public class ChatTypeCalculator : IChatTypeCalculator
    {
        private readonly ICharacterProvider _characterProvider;

        public ChatTypeCalculator(ICharacterProvider characterProvider)
        {
            _characterProvider = characterProvider;
        }

        public ChatType CalculateChatType(string input)
        {
            if(string.IsNullOrEmpty(input))
                return ChatType.Local;

            if(!CharacterIsAdmin && (input[0] == '+' || input[0] == '@'))
                return ChatType.Local;

            switch (input[0])
            {
                case '+': return ChatType.Admin;
                case '@': return ChatType.Announce;
                case '\'': return ChatType.Party;
                case '&': return ChatType.Guild;
                case '~': return ChatType.Global;
                case '!': return ChatType.PM;
                case '#': return ChatType.Command;
                default: return ChatType.Local;
            }
        }

        private bool CharacterIsAdmin => _characterProvider.MainCharacter.AdminLevel != AdminLevel.Player;
    }

    public interface IChatTypeCalculator
    {
        ChatType CalculateChatType(string input);
    }
}