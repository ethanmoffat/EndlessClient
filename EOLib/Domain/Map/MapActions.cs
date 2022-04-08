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
        private readonly ICurrentMapStateRepository _currentMapStateRepository;

        public MapActions(IPacketSendService packetSendService,
                          IItemPickupValidator itemPickupValidator,
                          ICharacterProvider characterProvider,
                          ICurrentMapStateRepository currentMapStateRepository)
        {
            _packetSendService = packetSendService;
            _itemPickupValidator = itemPickupValidator;
            _characterProvider = characterProvider;
            _currentMapStateRepository = currentMapStateRepository;
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

        public void OpenDoor(IWarp warp)
        {
            var packet = new PacketBuilder(PacketFamily.Door, PacketAction.Open)
                .AddChar((byte)warp.X)
                .AddChar((byte)warp.Y)
                .Build();

            _packetSendService.SendPacket(packet);
            _currentMapStateRepository.PendingDoors.Add(warp);
        }
    }

    public interface IMapActions
    {
        void RequestRefresh();

        ItemPickupResult PickUpItem(IItem item);

        void OpenDoor(IWarp warp);
    }
}
