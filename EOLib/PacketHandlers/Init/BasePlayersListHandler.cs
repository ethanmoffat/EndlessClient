using EOLib.Domain.Online;
using EOLib.Domain.Protocol;
using EOLib.Net;

namespace EOLib.PacketHandlers.Init
{
    public abstract class BasePlayersListHandler : IInitPacketHandler
    {
        private readonly IOnlinePlayerRepository _onlinePlayerRepository;

        public abstract InitReply Reply { get; }

        protected BasePlayersListHandler(IOnlinePlayerRepository onlinePlayerRepository)
        {
            _onlinePlayerRepository = onlinePlayerRepository;
        }

        public bool HandlePacket(IPacket packet)
        {
            var numTotal = packet.ReadShort();

            if (packet.ReadByte() != 255)
                return false;

            _onlinePlayerRepository.OnlinePlayers.Clear();
            for (int i = 0; i < numTotal; ++i)
            {
                _onlinePlayerRepository.OnlinePlayers.Add(GetNextRecord(packet));
            }

            return true;
        }

        protected abstract OnlinePlayerInfo GetNextRecord(IPacket packet);
    }
}
