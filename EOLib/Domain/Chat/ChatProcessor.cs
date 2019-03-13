// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using AutomaticTypeMapper;

namespace EOLib.Domain.Chat
{
    [AutoMappedType]
    public class ChatProcessor : IChatProcessor
    {
        public string RemoveFirstCharacterIfNeeded(string chat, ChatType chatType, string targetCharacter)
        {
            switch (chatType)
            {
                case ChatType.Command:
                case ChatType.NPC:
                case ChatType.Server:
                    throw new ArgumentOutOfRangeException(nameof(chatType));
                case ChatType.Admin:
                case ChatType.Global:
                case ChatType.Guild:
                case ChatType.Party:
                case ChatType.Announce:
                    return chat.Substring(1);
                case ChatType.PM:
                    chat = chat.Substring(1);
                    //todo: need to just send the whole string if the selected tab is the target character
                    //currently this is incorrect since it will remove the name
                    if (chat.ToLower().StartsWith(targetCharacter.ToLower()))
                        chat = chat.Substring(targetCharacter.Length);
                    return chat;
                case ChatType.Local:
                    return chat;
                default:
                    throw new ArgumentOutOfRangeException(nameof(chatType));
            }
        }
    }

    public interface IChatProcessor
    {
        string RemoveFirstCharacterIfNeeded(string input, ChatType chatType, string targetCharacter);
    }
}
