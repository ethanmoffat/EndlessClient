using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.IO.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Door
{
    /// <summary>
    /// Sent when a door is locked
    /// </summary>
    [AutoMappedType]
    public class DoorCloseHandler : InGameOnlyPacketHandler<DoorCloseServerPacket>
    {
        private readonly IEnumerable<IDoorEventNotifier> _doorEventNotifiers;

        public override PacketFamily Family => PacketFamily.Door;

        public override PacketAction Action => PacketAction.Close;

        public DoorCloseHandler(IPlayerInfoProvider playerInfoProvider,
                                IEnumerable<IDoorEventNotifier> doorEventNotifiers)
            : base(playerInfoProvider)
        {
            _doorEventNotifiers = doorEventNotifiers;
        }

        public override bool HandlePacket(DoorCloseServerPacket packet)
        {
            foreach (var notifier in _doorEventNotifiers)
                notifier.NotifyDoorLocked((ChestKey)packet.Key);

            return true;
        }
    }
}
