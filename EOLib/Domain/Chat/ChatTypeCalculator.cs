// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Domain.Character;

namespace EOLib.Domain.Chat
{
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
                throw new ArgumentException("Input string must have a value and be non-empty", nameof(input));

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