using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Locker
{
    /// <summary>
    /// Handles LOCKER_OPEN from server for opening a locker
    /// </summary>
    [AutoMappedType]
    public class LockerOpenHandler : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            var x = packet.ReadChar();
            var y = packet.ReadChar();

            _lockerDataRepository.ResetState();
            _lockerDataRepository.Location = new MapCoordinate(x, y);

            while (packet.ReadPosition < packet.Length)
                _lockerDataRepository.Items.Add(new InventoryItem(packet.ReadShort(), packet.ReadThree()));

            foreach (var notifier in _userInterfaceNotifiers)
                notifier.NotifyPacketDialog(Family);

            return true;
        }
    }
}
