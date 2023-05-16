using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Item;
using EOLib.IO.Map;
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
            var packet = new PacketBuilder(PacketFamily.Refresh, PacketAction.Request)
                .AddByte(255)
                .Build();
            _packetSendService.SendPacket(packet);
        }

        public ItemPickupResult PickUpItem(MapItem item)
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

        public void OpenDoor(Warp warp)
        {
            if (_currentMapStateRepository.PendingDoors.Contains(warp))
                return;

            var packet = new PacketBuilder(PacketFamily.Door, PacketAction.Open)
                .AddChar(warp.X)
                .AddChar(warp.Y)
                .Build();

            _packetSendService.SendPacket(packet);
            _currentMapStateRepository.PendingDoors.Add(warp);
        }

        public void OpenChest(MapCoordinate location)
        {
            var packet = new PacketBuilder(PacketFamily.Chest, PacketAction.Open)
                .AddChar(location.X)
                .AddChar(location.Y)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void OpenLocker(MapCoordinate location)
        {
            var packet = new PacketBuilder(PacketFamily.Locker, PacketAction.Open)
                .AddChar(location.X)
                .AddChar(location.Y)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void OpenBoard(TileSpec boardSpec)
        {
            var packet = new PacketBuilder(PacketFamily.Board, PacketAction.Open)
                .AddShort(boardSpec - TileSpec.Board1)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface IMapActions
    {
        void RequestRefresh();

        ItemPickupResult PickUpItem(MapItem item);

        void OpenDoor(Warp warp);

        void OpenChest(MapCoordinate location);

        void OpenLocker(MapCoordinate location);

        void OpenBoard(TileSpec boardSpec);
    }
}
