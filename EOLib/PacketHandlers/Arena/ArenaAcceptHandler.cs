using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Arena
{
    /// <summary>
    /// Arena win message
    /// </summary>
    [AutoMappedType]
    public class ArenaAcceptHandler : InGameOnlyPacketHandler
    {
        private readonly IEnumerable<IArenaNotifier> _arenaNotifiers;

        public override PacketFamily Family => PacketFamily.Arena;

        public override PacketAction Action => PacketAction.Accept;

        public ArenaAcceptHandler(IPlayerInfoProvider playerInfoProvider,
                                  IEnumerable<IArenaNotifier> arenaNotifiers)
            : base(playerInfoProvider)
        {
            _arenaNotifiers = arenaNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var winnerName = packet.ReadBreakString();

            var killsCount = packet.ReadInt();
            packet.ReadByte();

            var killerName = packet.ReadBreakString();
            var victimName = packet.ReadBreakString();

            foreach (var notifier in _arenaNotifiers)
            {
                notifier.NotifyArenaKill(killsCount, killerName, victimName);
                notifier.NotifyArenaWin(winnerName);
            }

            return true;
        }
    }
}
