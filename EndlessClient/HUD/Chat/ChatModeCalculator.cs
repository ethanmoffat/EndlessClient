// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using AutomaticTypeMapper;
using EndlessClient.UIControls;
using EOLib.Domain.Character;

namespace EndlessClient.HUD.Chat
{
    [MappedType(BaseType = typeof(IChatModeCalculator))]
    public class ChatModeCalculator : IChatModeCalculator
    {
        private readonly ICharacterProvider _characterProvider;

        public ChatModeCalculator(ICharacterProvider characterProvider)
        {
            _characterProvider = characterProvider;
        }

        public ChatModePictureBox.ChatMode CalculateMode(string fullTextString)
        {
            if(fullTextString == null)
                throw new ArgumentException("Input string is null!", nameof(fullTextString));
            if (fullTextString.Length == 0)
                return ChatModePictureBox.ChatMode.NoText;

            var playerIsAdmin = _characterProvider.MainCharacter.AdminLevel != AdminLevel.Player;
            var playerIsInGuild = !string.IsNullOrEmpty(_characterProvider.MainCharacter.GuildName);

            if (((fullTextString[0] == '@' || fullTextString[0] == '+') && !playerIsAdmin) ||
                (fullTextString[0] == '&' && !playerIsInGuild))
                return ChatModePictureBox.ChatMode.Public;

            switch (fullTextString[0])
            {
                case '!': return ChatModePictureBox.ChatMode.Private;
                case '@':
                case '~': return  ChatModePictureBox.ChatMode.Global;
                case '+': return ChatModePictureBox.ChatMode.Admin;
                case '\'': return ChatModePictureBox.ChatMode.Group;
                case '&': return ChatModePictureBox.ChatMode.Guild;
                default: return ChatModePictureBox.ChatMode.Public;
            }
        }
    }
}