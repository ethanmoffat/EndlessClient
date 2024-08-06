using System.Collections.Generic;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.IO.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Chest
{
    public class ChestCloseHandler : InGameOnlyPacketHandler<ChestCloseServerPacket>
    {
        private readonly IEnumerable<IChestEventNotifier> _chestEventNotifiers;

        public override PacketFamily Family => PacketFamily.Chest;

        public override PacketAction Action => PacketAction.Close;

        public ChestCloseHandler(IPlayerInfoProvider playerInfoProvider,
                                 IEnumerable<IChestEventNotifier> chestEventNotifiers)
            : base(playerInfoProvider)
        {
            _chestEventNotifiers = chestEventNotifiers;
        }

        public override bool HandlePacket(ChestCloseServerPacket packet)
        {
            if (packet.ByteSize < 2)
            {
                foreach (var notifier in _chestEventNotifiers)
                    notifier.NotifyChestBroken();
            }
            else
            {
                foreach (var notifier in _chestEventNotifiers)
                    notifier.NotifyChestLocked((ChestKey)(packet.Key ?? 0));
            }

            return true;
        }
    }
}
