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
        private readonly IChestDataProvider _chestDataProvider;

        public MapActions(IPacketSendService packetSendService,
                          IItemPickupValidator itemPickupValidator,
                          ICharacterProvider characterProvider,
                          ICurrentMapStateRepository currentMapStateRepository,
                          IChestDataProvider chestDataProvider)
        {
            _packetSendService = packetSendService;
            _itemPickupValidator = itemPickupValidator;
            _characterProvider = characterProvider;
            _currentMapStateRepository = currentMapStateRepository;
            _chestDataProvider = chestDataProvider;
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

        public void OpenChest(byte x, byte y)
        {
            var packet = new PacketBuilder(PacketFamily.Chest, PacketAction.Open)
                .AddChar(x)
                .AddChar(y)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void AddItemToChest(IInventoryItem item)
        {
            var packet = new PacketBuilder(PacketFamily.Chest, PacketAction.Add)
                .AddChar((byte)_chestDataProvider.Location.X)
                .AddChar((byte)_chestDataProvider.Location.Y)
                .AddShort(item.ItemID)
                .AddThree(item.Amount)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void TakeItemFromChest(short itemId)
        {
            var packet = new PacketBuilder(PacketFamily.Chest, PacketAction.Take)
                .AddChar((byte)_chestDataProvider.Location.X)
                .AddChar((byte)_chestDataProvider.Location.Y)
                .AddShort(itemId)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface IMapActions
    {
        void RequestRefresh();

        ItemPickupResult PickUpItem(IItem item);

        void OpenDoor(IWarp warp);

        void OpenChest(byte x, byte y);

        void AddItemToChest(IInventoryItem item);

        void TakeItemFromChest(short itemId);
    }
}
