using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Locker
{
    /// <summary>
    /// Handles LOCKER_OPEN from server for opening a locker
    /// </summary>
    [AutoMappedType]
    public class LockerOpenHandler : InGameOnlyPacketHandler<LockerOpenServerPacket>
    {
        private readonly ILockerDataRepository _lockerDataRepository;
        private readonly IEnumerable<IUserInterfaceNotifier> _userInterfaceNotifiers;

        public override PacketFamily Family => PacketFamily.Locker;

        public override PacketAction Action => PacketAction.Open;

        public LockerOpenHandler(IPlayerInfoProvider playerInfoProvider,
                                 ILockerDataRepository lockerDataRepository,
                                 IEnumerable<IUserInterfaceNotifier> userInterfaceNotifiers)
            : base(playerInfoProvider)
        {
            _lockerDataRepository = lockerDataRepository;
            _userInterfaceNotifiers = userInterfaceNotifiers;
        }

        public override bool HandlePacket(LockerOpenServerPacket packet)
        {
            _lockerDataRepository.ResetState();
            _lockerDataRepository.Location = new MapCoordinate(packet.LockerCoords.X, packet.LockerCoords.Y);

            foreach (var item in packet.LockerItems)
                _lockerDataRepository.Items.Add(new InventoryItem(item.Id, item.Amount));

            foreach (var notifier in _userInterfaceNotifiers)
                notifier.NotifyPacketDialog(Family);

            return true;
        }
    }
}