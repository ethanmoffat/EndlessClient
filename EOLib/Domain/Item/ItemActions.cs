using System.IO;
using AutomaticTypeMapper;
using EOLib.Domain.Map;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Data;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace EOLib.Domain.Item
{
    [AutoMappedType]
    public class ItemActions : IItemActions
    {
        private readonly IPacketSendService _packetSendService;

        public ItemActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void UseItem(int itemId)
        {
            var packet = new ItemUseClientPacket { ItemId = itemId };
            _packetSendService.SendPacket(packet);
        }

        public void EquipItem(int itemId, bool alternateLocation)
        {
            var packet = new PaperdollAddClientPacket
            {
                ItemId = itemId,
                SubLoc = alternateLocation ? 1 : 0,
            };
            _packetSendService.SendPacket(packet);
        }

        public void UnequipItem(int itemId, bool alternateLocation)
        {
            var packet = new PaperdollRemoveClientPacket
            {
                ItemId = itemId,
                SubLoc = alternateLocation ? 1 : 0,
            };
            _packetSendService.SendPacket(packet);
        }

        public void DropItem(int itemId, int amount, MapCoordinate dropPoint)
        {
            // The drop point coordinates are extra encoded. See ItemDropClientPacket::Coords.
            var dropX = dropPoint.X == 255 ? dropPoint.X : NumberEncoder.EncodeNumber(dropPoint.X)[0];
            var dropY = dropPoint.Y == 255 ? dropPoint.Y : NumberEncoder.EncodeNumber(dropPoint.Y)[0];

            var packet = new ItemDropClientPacket
            {
                Item = new ThreeItem
                {
                    Id = itemId,
                    Amount = amount,
                },
                Coords = new ByteCoords { X = dropX, Y = dropY },
            };
            _packetSendService.SendPacket(packet);
        }

        public void JunkItem(int itemId, int amount)
        {
            var packet = new ItemJunkClientPacket
            {
                Item = new Moffat.EndlessOnline.SDK.Protocol.Net.Item
                {
                    Id = itemId,
                    Amount = amount,
                }
            };
            _packetSendService.SendPacket(packet);
        }
    }

    public interface IItemActions
    {
        void UseItem(int itemId);

        void EquipItem(int itemId, bool alternateLocation);

        void UnequipItem(int itemId, bool alternateLocation);

        void DropItem(int itemId, int amount, MapCoordinate dropPoint);

        void JunkItem(int itemId, int amount);
    }
}