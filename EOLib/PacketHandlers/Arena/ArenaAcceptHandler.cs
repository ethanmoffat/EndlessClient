using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Arena
{
    /// <summary>
    /// Arena win message
    /// </summary>
    [AutoMappedType]
    public class ArenaAcceptHandler : InGameOnlyPacketHandler<ArenaAcceptServerPacket>
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

        public override bool HandlePacket(ArenaAcceptServerPacket packet)
        {
            foreach (var notifier in _arenaNotifiers)
            {
                notifier.NotifyArenaKill(packet.KillsCount, packet.KillerName, packet.VictimName);
                notifier.NotifyArenaWin(packet.WinnerName);
            }

            return true;
        }
    }
}
