using System;
using AutomaticTypeMapper;
using EOLib.Domain.Chat;

namespace EOLib.Net.Builders
{
    [AutoMappedType]
    public class ChatPacketBuilder : IChatPacketBuilder
    {
        public IPacket BuildChatPacket(ChatType chatType,
            string chat,
            string targetCharacter)
        {
            IPacketBuilder packetBuilder = new PacketBuilder(PacketFamily.Talk, GetActionFromType(chatType));
            
            if (chatType == ChatType.PM)
            {
                if(string.IsNullOrEmpty(targetCharacter))
                    throw new ArgumentException("Target character for PM must not be null or empty", nameof(targetCharacter));
                packetBuilder = packetBuilder.AddBreakString(targetCharacter);
            }

            packetBuilder = packetBuilder.AddString(chat);
            return packetBuilder.Build();
        }

        private PacketAction GetActionFromType(ChatType chatType)
        {
            switch (chatType)
            {
                case ChatType.Local: return PacketAction.Report;
                case ChatType.PM: return PacketAction.Tell;
                case ChatType.Global: return PacketAction.Message;
                case ChatType.Guild: return PacketAction.Request;
                case ChatType.Party: return PacketAction.Open;
                case ChatType.Admin: return PacketAction.Admin;
                case ChatType.Announce: return PacketAction.Announce;
                default: throw new ArgumentOutOfRangeException(nameof(chatType));
            }
        }
    }
}