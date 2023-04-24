using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Map
{
    [AutoMappedType]
    public class ChestActions : IChestActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IChestDataProvider _chestDataProvider;

        public ChestActions(IPacketSendService packetSendService,
                            IChestDataProvider chestDataProvider)
        {
            _packetSendService = packetSendService;
            _chestDataProvider = chestDataProvider;
        }

        public void AddItemToChest(InventoryItem item)
        {
            var packet = new PacketBuilder(PacketFamily.Chest, PacketAction.Add)
                .AddChar(_chestDataProvider.Location.X)
                .AddChar(_chestDataProvider.Location.Y)
                .AddShort(item.ItemID)
                .AddThree(item.Amount)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void TakeItemFromChest(int itemId)
        {
            var packet = new PacketBuilder(PacketFamily.Chest, PacketAction.Take)
                .AddChar(_chestDataProvider.Location.X)
                .AddChar(_chestDataProvider.Location.Y)
                .AddShort(itemId)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface IChestActions
    {

        void AddItemToChest(InventoryItem item);

        void TakeItemFromChest(int itemId);
    }
}
