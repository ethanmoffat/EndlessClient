using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Item;
using EOLib.IO.Map;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

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

        public void RequestRefresh() => _packetSendService.SendPacket(new RefreshRequestClientPacket());

        public ItemPickupResult PickUpItem(MapItem item)
        {
            var pickupResult = _itemPickupValidator.ValidateItemPickup(_characterProvider.MainCharacter, item);
            if (pickupResult == ItemPickupResult.Ok)
            {
                var packet = new ItemGetClientPacket { ItemIndex = item.UniqueID };
                _packetSendService.SendPacket(packet);
            }

            return pickupResult;
        }

        public void OpenDoor(Warp warp)
        {
            if (_currentMapStateRepository.PendingDoors.Contains(warp))
                return;

            var packet = new DoorOpenClientPacket { Coords = new Coords { X = warp.X, Y = warp.Y } };
            _packetSendService.SendPacket(packet);
            _currentMapStateRepository.PendingDoors.Add(warp);
        }

        public void OpenChest(MapCoordinate location)
        {
            var packet = new ChestOpenClientPacket { Coords = new Coords { X = location.X, Y = location.Y } };
            _packetSendService.SendPacket(packet);
        }

        public void OpenLocker(MapCoordinate location)
        {
            var packet = new LockerOpenClientPacket { LockerCoords = new Coords { X = location.X, Y = location.Y } };
            _packetSendService.SendPacket(packet);
        }

        public void OpenBoard(TileSpec boardSpec)
        {
            var packet = new BoardOpenClientPacket { BoardId = boardSpec - TileSpec.Board1 };
            _packetSendService.SendPacket(packet);
        }

        public void OpenJukebox(MapCoordinate location)
        {
            var packet = new JukeboxOpenClientPacket { Coords = new Coords { X = location.X, Y = location.Y } };
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

        void OpenJukebox(MapCoordinate location);
    }
}
