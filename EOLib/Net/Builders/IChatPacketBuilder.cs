using EOLib.Domain.Chat;

namespace EOLib.Net.Builders
{
    public interface IChatPacketBuilder
    {
        IPacket BuildChatPacket(ChatType chatType,
                                  string chat,
                                  string targetCharacter);
    }
}
