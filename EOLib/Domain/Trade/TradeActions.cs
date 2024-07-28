using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace EOLib.Domain.Trade
{
    [AutoMappedType]
    public class TradeActions : ITradeActions
    {
        private readonly IPacketSendService _packetSendService;

        public TradeActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void RequestTrade(int characterID)
        {
            var packet = new TradeRequestClientPacket { PlayerId = characterID };
            _packetSendService.SendPacket(packet);
        }

        public void AcceptTradeRequest(int characterID)
        {
            var packet = new TradeAcceptClientPacket { PlayerId = characterID };
            _packetSendService.SendPacket(packet);
        }

        public void RemoveItemFromOffer(int itemID)
        {
            var packet = new TradeRemoveClientPacket { ItemId = itemID };
            _packetSendService.SendPacket(packet);
        }

        public void AddItemToOffer(int itemID, int amount)
        {
            var packet = new TradeAddClientPacket
            {
                AddItem = new Moffat.EndlessOnline.SDK.Protocol.Net.Item
                {
                    Id = itemID,
                    Amount = amount,
                }
            };
            _packetSendService.SendPacket(packet);
        }

        public void AgreeToTrade(bool agree)
        {
            var packet = new TradeAgreeClientPacket { Agree = agree };
            _packetSendService.SendPacket(packet);
        }

        public void CancelTrade() => _packetSendService.SendPacket(new TradeCloseClientPacket());
    }

    public interface ITradeActions
    {
        void RequestTrade(int characterID);

        void AcceptTradeRequest(int characterID);

        void RemoveItemFromOffer(int itemID);

        void AddItemToOffer(int itemID, int amount);

        void AgreeToTrade(bool agree);

        void CancelTrade();
    }
}