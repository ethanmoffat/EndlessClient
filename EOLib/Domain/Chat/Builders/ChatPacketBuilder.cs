using System;
using AutomaticTypeMapper;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace EOLib.Domain.Chat.Builders
{
    [AutoMappedType]
    public class ChatPacketBuilder : IChatPacketBuilder
    {
        public IPacket BuildChatPacket(ChatType chatType, string chat, string targetCharacter)
        {
            IPacket packet;
            switch (chatType)
            {
                case ChatType.Local: packet = new TalkReportClientPacket { Message = chat }; break;
                case ChatType.PM:
                    if (string.IsNullOrEmpty(targetCharacter))
                    {
                        throw new ArgumentException("Target character must be specified for PM messages", nameof(targetCharacter));
                    }
                    packet = new TalkTellClientPacket { Message = chat, Name = targetCharacter };
                    break;
                case ChatType.Global: packet = new TalkMsgClientPacket { Message = chat }; break;
                case ChatType.Guild: packet = new TalkRequestClientPacket { Message = chat }; break;
                case ChatType.Party: packet = new TalkOpenClientPacket { Message = chat }; break;
                case ChatType.Admin: packet = new TalkAdminClientPacket { Message = chat }; break;
                case ChatType.Announce: packet = new TalkAnnounceClientPacket { Message = chat }; break;
                default: throw new ArgumentOutOfRangeException(nameof(chatType));
            }
            return packet;
        }
    }
}