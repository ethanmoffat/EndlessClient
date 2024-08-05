using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

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
            var packet = new ChestAddClientPacket
            {
                Coords = new Coords
                {
                    X = _chestDataProvider.Location.X,
                    Y = _chestDataProvider.Location.Y,
                },
                AddItem = new ThreeItem
                {
                    Id = item.ItemID,
                    Amount = item.Amount
                }
            };
            _packetSendService.SendPacket(packet);
        }

        public void TakeItemFromChest(int itemId)
        {
            var packet = new ChestTakeClientPacket
            {
                Coords = new Coords
                {
                    X = _chestDataProvider.Location.X,
                    Y = _chestDataProvider.Location.Y,
                },
                TakeItemId = itemId
            };
            _packetSendService.SendPacket(packet);
        }
    }

    public interface IChestActions
    {

        void AddItemToChest(InventoryItem item);

        void TakeItemFromChest(int itemId);
    }
}