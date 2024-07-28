using Moffat.EndlessOnline.SDK.Protocol.Net;
using System.Threading.Tasks;

namespace EOLib.Net.Handlers
{
    public abstract class DefaultAsyncPacketHandler<TPacket> : IPacketHandler<TPacket>
        where TPacket : IPacket
    {
        public abstract PacketFamily Family { get; }

        public abstract PacketAction Action { get; }

        public abstract bool CanHandle { get; }

        public bool IsHandlerFor(IPacket packet)
        {
            return typeof(TPacket).IsAssignableFrom(packet.GetType());
        }

        public bool HandlePacket(IPacket packet)
        {
            return HandlePacket((TPacket)packet);
        }

        public abstract bool HandlePacket(TPacket packet);

        public async Task<bool> HandlePacketAsync(TPacket packet)
        {
            return await Task.Run(() => HandlePacket(packet));
        }
    }
}