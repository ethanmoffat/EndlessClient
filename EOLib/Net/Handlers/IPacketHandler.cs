using System.Threading.Tasks;

namespace EOLib.Net.Handlers
{
    public interface IPacketHandler
    {
        PacketFamily Family { get; }
        PacketAction Action { get; }

        bool CanHandle { get; }

        bool HandlePacket(IPacket packet);

        //todo: method to determine whether a packet should be handled asynchronously or not
        Task<bool> HandlePacketAsync(IPacket packet);
    }
}
