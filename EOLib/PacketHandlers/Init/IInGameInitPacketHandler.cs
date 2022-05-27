using EOLib.Domain.Protocol;
using EOLib.Net;

namespace EOLib.PacketHandlers.Init
{
    public interface IInitPacketHandler
    {
        InitReply Reply { get; }

        bool HandlePacket(IPacket packet);
    }
}
