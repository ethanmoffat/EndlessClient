using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Arena
{
    /// <summary>
    /// Arena start message
    /// </summary>
    [AutoMappedType]
    public class ArenaUseHandler : InGameOnlyPacketHandler<ArenaUseServerPacket>
    {
        private readonly IEnumerable<IArenaNotifier> _arenaNotifiers;

        public override PacketFamily Family => PacketFamily.Arena;

        public override PacketAction Action => PacketAction.Use;

        public ArenaUseHandler(IPlayerInfoProvider playerInfoProvider,
                               IEnumerable<IArenaNotifier> arenaNotifiers)
            : base(playerInfoProvider)
        {
            _arenaNotifiers = arenaNotifiers;
        }

        public override bool HandlePacket(ArenaUseServerPacket packet)
        {
            foreach (var notifier in _arenaNotifiers)
            {
                notifier.NotifyArenaStart(packet.PlayersCount);
            }

            return true;
        }
    }
}