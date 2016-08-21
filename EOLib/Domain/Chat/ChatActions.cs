// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using EOLib.Domain.Character;
using EOLib.Net.Builders;
using EOLib.Net.Communication;

namespace EOLib.Domain.Chat
{
    public class ChatActions : IChatActions
    {
        private readonly IChatRepository _chatRepository;
        private readonly ICharacterProvider _characterProvider;
        private readonly IChatTypeCalculator _chatTypeCalculator;
        private readonly IChatPacketBuilder _chatPacketBuilder;
        private readonly IPacketSendService _packetSendService;
        private readonly ILocalCommandHandler _localCommandHandler;

        public ChatActions(IChatRepository chatRepository,
                           ICharacterProvider characterProvider,
                           IChatTypeCalculator chatTypeCalculator,
                           IChatPacketBuilder chatPacketBuilder,
                           IPacketSendService packetSendService,
                           ILocalCommandHandler localCommandHandler)
        {
            _chatRepository = chatRepository;
            _characterProvider = characterProvider;
            _chatTypeCalculator = chatTypeCalculator;
            _chatPacketBuilder = chatPacketBuilder;
            _packetSendService = packetSendService;
            _localCommandHandler = localCommandHandler;
        }

        public async Task SendChatToServer()
        {
            //todo: get target character for PM
            await SendChatToServer(_chatRepository.LocalTypedText);
        }

        public async Task SendChatToServer(string chat, string targetCharacter = null)
        {
            var chatType = _chatTypeCalculator.CalculateChatType(chat);

            if (chatType == ChatType.Command)
            {
                if (HandleCommand(chat))
                    return;

                //treat unhandled command as local chat
                chatType = ChatType.Local;
            }

            chat = RemoveFirstCharacterIfNeeded(chat, chatType);

            var chatPacket = _chatPacketBuilder.BuildChatPacket(chatType, chat, targetCharacter);
            await _packetSendService.SendPacketAsync(chatPacket);

            AddChatForLocalDisplay(chatType, chat);
        }

        /// <summary>
        /// Returns true if the text is a command (#) and can be handled as such, false if it should be sent to the server as a public chat string
        /// </summary>
        private bool HandleCommand(string commandString)
        {
            var command = new string(commandString.Substring(1)
                .TakeWhile(x => x != ' ')
                .ToArray())
                .Trim();
            var parameters = new string(commandString.SkipWhile(x => x != ' ')
                .ToArray())
                .Trim();
            return _localCommandHandler.HandleCommand(command, parameters);
        }

        private string RemoveFirstCharacterIfNeeded(string chat, ChatType chatType)
        {
            //todo: handling for PMs
            switch (chatType)
            {
                case ChatType.Command:
                case ChatType.NPC:
                case ChatType.Server:
                    throw new ArgumentOutOfRangeException("chatType");
                case ChatType.Admin:
                case ChatType.Global:
                case ChatType.Guild:
                case ChatType.Party:
                case ChatType.Announce:
                    return chat.Substring(1);
                default:
                    return chat;
            }
        }

        private void AddChatForLocalDisplay(ChatType chatType, string chat)
        {
            //todo: handling for speech bubbles - announce, local, and group (and maybe guild?) need it.
            //todo: need some sort of event that fires or client-side detection mechanism (it should not be known about here)
            //todo: the same detection mechanism should also be used when other players chat is handled
            var who = _characterProvider.MainCharacter.Name;
            switch (chatType)
            {
                case ChatType.Admin:
                    _chatRepository.AllChat[ChatTab.Group].Add(new ChatData(who, chat, ChatIcon.HGM, ChatColor.Admin));
                    break;
                case ChatType.PM:
                    //todo: handling for PM
                    break;
                case ChatType.Local:
                    _chatRepository.AllChat[ChatTab.Local].Add(new ChatData(who, chat));
                    break;
                case ChatType.Global:
                    _chatRepository.AllChat[ChatTab.Global].Add(new ChatData(who, chat));
                    break;
                case ChatType.Guild:
                    //todo: there are special cases here for guild chat that aren't handled
                    _chatRepository.AllChat[ChatTab.Group].Add(new ChatData(who, chat));
                    break;
                case ChatType.Party:
                    _chatRepository.AllChat[ChatTab.Local].Add(new ChatData(who, chat, ChatIcon.PlayerPartyDark, ChatColor.PM));
                    _chatRepository.AllChat[ChatTab.Group].Add(new ChatData(who, chat, ChatIcon.PlayerPartyDark));
                    break;
                case ChatType.Announce:
                    _chatRepository.AllChat[ChatTab.Local].Add(new ChatData(who, chat, ChatIcon.GlobalAnnounce, ChatColor.ServerGlobal));
                    _chatRepository.AllChat[ChatTab.Global].Add(new ChatData(who, chat, ChatIcon.GlobalAnnounce, ChatColor.ServerGlobal));
                    _chatRepository.AllChat[ChatTab.Group].Add(new ChatData(who, chat, ChatIcon.GlobalAnnounce, ChatColor.ServerGlobal));
                    break;
                default:
                    throw new ArgumentOutOfRangeException("chatType", chatType, "Unexpected ChatType encountered");
            }
        }
    }
}