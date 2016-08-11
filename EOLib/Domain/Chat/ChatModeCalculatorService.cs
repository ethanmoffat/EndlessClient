// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Domain.Chat
{
    public class ChatModeCalculatorService : IChatModeCalculatorService
    {
        public ChatMode CalculateChatType(string fullTextString, bool playerIsAdmin, bool playerIsInGuild)
        {
            if(string.IsNullOrEmpty(fullTextString))
                throw new ArgumentException("Input string is null or empty!", "fullTextString");

            if ((fullTextString[0] == '@' || fullTextString[0] == '+') && !playerIsAdmin)
                return ChatMode.Public;

            if (fullTextString[0] == '&' && !playerIsInGuild)
                return ChatMode.Public;

            switch (fullTextString[0])
            {
                case '!': return ChatMode.Private;
                case '@': return ChatMode.Global;
                case '~': return  ChatMode.Global;
                case '+': return ChatMode.Admin;
                case '\'': return ChatMode.Group;
                case '&': return ChatMode.Guild;
                default: return ChatMode.Public;
            }
        }
    }
}