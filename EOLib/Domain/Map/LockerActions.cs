using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Optional.Collections;

namespace EOLib.Domain.Map
{
    [AutoMappedType]
    public class LockerActions : ILockerActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly ILockerDataProvider _lockerDataProvider;

        public LockerActions(IPacketSendService packetSendService,
                             ILockerDataProvider lockerDataProvider)
        {
            _packetSendService = packetSendService;
            _lockerDataProvider = lockerDataProvider;
        }

        public void AddItemToLocker(InventoryItem item)
        {
            var packet = new LockerAddClientPacket
            {
                LockerCoords = new Coords
                {
                    X = _lockerDataProvider.Location.X,
                    Y = _lockerDataProvider.Location.Y,
                },
                DepositItem = new ThreeItem
                {
                    Id = item.ItemID,
                    Amount = item.Amount,
                }
            };
            _packetSendService.SendPacket(packet);
        }

        public void TakeItemFromLocker(int itemId)
        {
            var packet = new LockerTakeClientPacket
            {
                LockerCoords = new Coords
                {
                    X = _lockerDataProvider.Location.X,
                    Y = _lockerDataProvider.Location.Y,
                },
                TakeItemId = itemId
            };
            _packetSendService.SendPacket(packet);
        }

        public int GetNewItemAmount(int itemId, int amount)
        {
            return _lockerDataProvider.Items
                .SingleOrNone(x => x.ItemID == itemId)
                .Match(item => item.Amount + amount, () => amount);
        }
    }

    public interface ILockerActions
    {
        void AddItemToLocker(InventoryItem item);

        void TakeItemFromLocker(int itemId);

        int GetNewItemAmount(int itemId, int amount);
    }
}