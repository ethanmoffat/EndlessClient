// Original Work Copyright (c) Ethan Moffat 2014-2019

using AutomaticTypeMapper;
using EOLib.Domain.Item;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Map
{
    [AutoMappedType]
    public class MapActions : IMapActions
    {
        private readonly IPacketSendService _packetSendService;

        public MapActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public ItemPickupResult PickUpItem(IItem item)
        {
            //todo: validation that item can be picked up (client-side drop protection)

            var packet = new PacketBuilder(PacketFamily.Item, PacketAction.Get)
                .AddShort(item.UniqueID)
                .Build();

            _packetSendService.SendPacket(packet);

            return ItemPickupResult.Ok;
        }
    }

    public interface IMapActions
    {
        ItemPickupResult PickUpItem(IItem item);
    }
}
