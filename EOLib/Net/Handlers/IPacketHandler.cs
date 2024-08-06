using System.Threading.Tasks;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Net.Handlers
{
    public interface IPacketHandler
    {
        PacketFamily Family { get; }
        PacketAction Action { get; }

        bool CanHandle { get; }

        bool IsHandlerFor(IPacket packet);

        bool HandlePacket(IPacket packet);
    }

    public interface IPacketHandler<TPacket> : IPacketHandler
        where TPacket : IPacket
    {
        bool HandlePacket(TPacket packet);

        //todo: method to determine whether a packet should be handled asynchronously or not
        Task<bool> HandlePacketAsync(TPacket packet);
    }
}
