using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Domain.Chat.Builders
{
    public interface IChatPacketBuilder
    {
        IPacket BuildChatPacket(ChatType chatType, string chat, string targetCharacter);
    }
}