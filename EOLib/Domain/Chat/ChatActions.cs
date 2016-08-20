// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using System.Threading.Tasks;
using EOLib.Net.Builders;
using EOLib.Net.Communication;

namespace EOLib.Domain.Chat
{
    public class ChatActions : IChatActions
    {
        private readonly IChatProvider _chatProvider;
        private readonly IChatTypeCalculator _chatTypeCalculator;
        private readonly IChatPacketBuilder _chatPacketBuilder;
        private readonly IPacketSendService _packetSendService;
        private readonly ILocalCommandHandler _localCommandHandler;

        public ChatActions(IChatProvider chatProvider,
            IChatTypeCalculator chatTypeCalculator,
            IChatPacketBuilder chatPacketBuilder,
            IPacketSendService packetSendService,
            ILocalCommandHandler localCommandHandler)
        {
            _chatProvider = chatProvider;
            _chatTypeCalculator = chatTypeCalculator;
            _chatPacketBuilder = chatPacketBuilder;
            _packetSendService = packetSendService;
            _localCommandHandler = localCommandHandler;
        }

        public async Task SendChatToServer()
        {
            //todo: get target character for PM
            await SendChatToServer(_chatProvider.LocalTypedText);
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
    }
}