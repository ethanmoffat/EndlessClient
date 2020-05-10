using System.Threading.Tasks;

namespace EOLib.Net.Handlers
{
    public abstract class DefaultAsyncPacketHandler : IPacketHandler
    {
        public abstract PacketFamily Family { get; }

        public abstract PacketAction Action { get; }

        public abstract bool CanHandle { get; }

        public abstract bool HandlePacket(IPacket packet);

        public async Task<bool> HandlePacketAsync(IPacket packet)
        {
            return await Task.Run(() => HandlePacket(packet));
        }
    }
}
