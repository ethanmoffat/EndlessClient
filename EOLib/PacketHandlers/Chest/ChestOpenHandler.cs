using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Chest
{
    [AutoMappedType]
    public class ChestOpenHandler : InGameOnlyPacketHandler<ChestOpenServerPacket>
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

        public override bool HandlePacket(ChestOpenServerPacket packet)
        {
            _chestDataRepository.ResetState();
            _chestDataRepository.Location = new MapCoordinate(packet.Coords.X, packet.Coords.Y);
            _chestDataRepository.Items = new HashSet<ChestItem>(packet.Items.Select((x, i) => new ChestItem(x.Id, x.Amount, i)));

            foreach (var notifier in _userInterfaceNotifiers)
                notifier.NotifyPacketDialog(Family);

            return true;
        }
    }
}