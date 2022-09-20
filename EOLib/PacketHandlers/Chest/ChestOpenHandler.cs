using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Chest
{
    [AutoMappedType]
    public class ChestOpenHandler : InGameOnlyPacketHandler
    {
        private readonly IChestDataRepository _chestDataRepository;
        private readonly IEnumerable<IUserInterfaceNotifier> _userInterfaceNotifiers;

        public override PacketFamily Family => PacketFamily.Chest;

        public override PacketAction Action => PacketAction.Open;

        public ChestOpenHandler(IPlayerInfoProvider playerInfoProvider,
                                IChestDataRepository chestDataRepository,
                                IEnumerable<IUserInterfaceNotifier> userInterfaceNotifiers)
            : base(playerInfoProvider)
        {
            _chestDataRepository = chestDataRepository;
            _userInterfaceNotifiers = userInterfaceNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var x = packet.ReadChar();
            var y = packet.ReadChar();

            _chestDataRepository.ResetState();
            _chestDataRepository.Location = new MapCoordinate(x, y);

            int i = 0;
            while (packet.ReadPosition < packet.Length)
                _chestDataRepository.Items.Add(new ChestItem(packet.ReadShort(), packet.ReadThree(), i++));

            foreach (var notifier in _userInterfaceNotifiers)
                notifier.NotifyPacketDialog(Family);

            return true;
        }
    }
}
