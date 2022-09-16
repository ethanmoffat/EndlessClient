using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;
using System;

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

        public void RequestTrade(short characterID)
        {
            var packet = new PacketBuilder(PacketFamily.Trade, PacketAction.Request)
                .AddChar(6)
                .AddShort(characterID)
                .Build();
            _packetSendService.SendPacket(packet);
        }

        public void AcceptTradeRequest(short characterID)
        {
            var packet = new PacketBuilder(PacketFamily.Trade, PacketAction.Accept)
                .AddChar(6)
                .AddShort(characterID)
                .Build();
            _packetSendService.SendPacket(packet);
        }

        public void RemoveItemFromOffer(short itemID)
        {
            var packet = new PacketBuilder(PacketFamily.Trade, PacketAction.Remove)
                .AddShort(itemID)
                .Build();
            _packetSendService.SendPacket(packet);
        }

        public void AddItemToOffer(short itemID, int amount)
        {
            var packet = new PacketBuilder(PacketFamily.Trade, PacketAction.Add)
                .AddShort(itemID)
                .AddInt(amount)
                .Build();
            _packetSendService.SendPacket(packet);
        }

        public void AgreeToTrade(bool agree)
        {
            var packet = new PacketBuilder(PacketFamily.Trade, PacketAction.Agree)
                .AddChar((byte)(agree ? 1 : 0))
                .Build();
            _packetSendService.SendPacket(packet);
        }

        public void CancelTrade()
        {
            var packet = new PacketBuilder(PacketFamily.Trade, PacketAction.Close)
                .AddChar(6)
                .Build();
            _packetSendService.SendPacket(packet);
        }
    }

    public interface ITradeActions
    {
        void RequestTrade(short characterID);

        void AcceptTradeRequest(short characterID);

        void RemoveItemFromOffer(short itemID);

        void AddItemToOffer(short itemID, int amount);

        void AgreeToTrade(bool agree);

        void CancelTrade();
    }
}
