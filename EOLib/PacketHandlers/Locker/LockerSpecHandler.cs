using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Locker
{
    [AutoMappedType]
    public class LockerSpecHandler : InGameOnlyPacketHandler<LockerSpecServerPacket>
    {
        private readonly IEnumerable<ILockerEventNotifier> _lockerEventNotifiers;

        public override PacketFamily Family => PacketFamily.Locker;

        public override PacketAction Action => PacketAction.Spec;

        public LockerSpecHandler(IPlayerInfoProvider playerInfoProvider,
                                 IEnumerable<ILockerEventNotifier> lockerEventNotifiers)
            : base(playerInfoProvider)
        {
            _lockerEventNotifiers = lockerEventNotifiers;
        }

        public override bool HandlePacket(LockerSpecServerPacket packet)
        {
            foreach (var notifier in _lockerEventNotifiers)
            {
                notifier.NotifyLockerFull(packet.LockerMaxItems);
            }

            return true;
        }
    }
}