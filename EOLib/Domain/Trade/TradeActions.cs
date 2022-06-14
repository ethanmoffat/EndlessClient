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
        private readonly ITradeRepository _tradeRepository;
        private readonly Random _random;

        public TradeActions(IPacketSendService packetSendService,
                            ITradeRepository tradeRepository)
        {
            _packetSendService = packetSendService;
            _tradeRepository = tradeRepository;
            _random = new Random();
        }

        public void RequestTrade(short characterID)
        {
            var tradeId = (byte)(_random.Next(252) + 1);
            _tradeRepository.TradeSessionID = tradeId;

            var packet = new PacketBuilder(PacketFamily.Trade, PacketAction.Request)
                .AddChar(tradeId)
                .AddShort(characterID)
                .Build();
            _packetSendService.SendPacket(packet);
        }

        public void AcceptTradeRequest(short characterID)
        {
            var packet = new PacketBuilder(PacketFamily.Trade, PacketAction.Accept)
                .AddChar(_tradeRepository.TradeSessionID)
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
                .AddChar(_tradeRepository.TradeSessionID)
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
