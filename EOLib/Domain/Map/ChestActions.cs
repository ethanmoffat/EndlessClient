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

    public interface IChestActions
    {

        void AddItemToChest(IInventoryItem item);

        void TakeItemFromChest(short itemId);
    }
}
