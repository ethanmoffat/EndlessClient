using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional;
using System;

namespace EOLib.PacketHandlers.Items
{
    [AutoMappedType]
    public class OtherPlayerDropItemHandler : InGameOnlyPacketHandler
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.Item;

        public override PacketAction Action => PacketAction.Add;

        public OtherPlayerDropItemHandler(IPlayerInfoProvider playerInfoProvider,
                                          ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var id = packet.ReadShort();
            var uid = packet.ReadShort();

            var amountDropped = packet.ReadThree();
            var dropX = packet.ReadChar();
            var dropY = packet.ReadChar();

            var mapItem = new MapItem(uid, id, dropX, dropY, amountDropped)
                .WithDropTime(Option.Some(DateTime.Now));
            _currentMapStateRepository.MapItems.Add(mapItem);

            return true;
        }
    }
}
