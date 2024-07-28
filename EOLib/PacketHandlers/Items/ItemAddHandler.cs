using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using System;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when another player drops an item on the map
    /// </summary>
    [AutoMappedType]
    public class ItemAddHandler : InGameOnlyPacketHandler<ItemAddServerPacket>
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public override PacketFamily Family => PacketFamily.Item;

        public override PacketAction Action => PacketAction.Add;

        public ItemAddHandler(IPlayerInfoProvider playerInfoProvider,
                              ICurrentMapStateRepository currentMapStateRepository)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
        }

        public override bool HandlePacket(ItemAddServerPacket packet)
        {
            var mapItem = new MapItem(packet.ItemIndex, packet.ItemId, packet.Coords.X, packet.Coords.Y, packet.ItemAmount)
                .WithDropTime(Option.Some(DateTime.Now));
            _currentMapStateRepository.MapItems.Add(mapItem);

            return true;
        }
    }
}