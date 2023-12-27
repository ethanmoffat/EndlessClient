using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Net;
using EOLib.Net.Communication;
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
            var packet = new PacketBuilder(PacketFamily.Locker, PacketAction.Add)
                .AddChar(_lockerDataProvider.Location.X)
                .AddChar(_lockerDataProvider.Location.Y)
                .AddShort(item.ItemID)
                .AddThree(item.Amount)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void TakeItemFromLocker(int itemId)
        {
            var packet = new PacketBuilder(PacketFamily.Locker, PacketAction.Take)
                .AddChar(_lockerDataProvider.Location.X)
                .AddChar(_lockerDataProvider.Location.Y)
                .AddShort(itemId)
                .Build();

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
