using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Item;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Map
{
    [AutoMappedType]
    public class MapActions : IMapActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IItemPickupValidator _itemPickupValidator;
        private readonly ICharacterProvider _characterProvider;

        public MapActions(IPacketSendService packetSendService,
                          IItemPickupValidator itemPickupValidator,
                          ICharacterProvider characterProvider)
        {
            _packetSendService = packetSendService;
            _itemPickupValidator = itemPickupValidator;
            _characterProvider = characterProvider;
        }

        public void RequestRefresh()
        {
            var packet = new PacketBuilder(PacketFamily.Refresh, PacketAction.Request).Build();
            _packetSendService.SendPacket(packet);
        }

        public ItemPickupResult PickUpItem(IItem item)
        {
            var pickupResult = _itemPickupValidator.ValidateItemPickup(_characterProvider.MainCharacter, item);
            if (pickupResult == ItemPickupResult.Ok)
            {
                var packet = new PacketBuilder(PacketFamily.Item, PacketAction.Get)
                    .AddShort(item.UniqueID)
                    .Build();

                _packetSendService.SendPacket(packet);
            }

            return pickupResult;
        }
    }

    public interface IMapActions
    {
        void RequestRefresh();

        ItemPickupResult PickUpItem(IItem item);
    }
}
