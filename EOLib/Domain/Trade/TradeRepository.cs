using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;

namespace EOLib.Domain.Trade
{
    public interface ITradeRepository : IResettable
    {
        TradeOffer PlayerOneOffer { get; set; }

        TradeOffer PlayerTwoOffer { get; set; }
    }

    public interface ITradeProvider
    {
        TradeOffer PlayerOneOffer { get; }

        TradeOffer PlayerTwoOffer { get; }
    }

    [AutoMappedType(IsSingleton = true)]
    public class TradeRepository : ITradeRepository, ITradeProvider
    {
        public TradeOffer PlayerOneOffer { get; set; }

        public TradeOffer PlayerTwoOffer { get; set; }

        public TradeRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            PlayerOneOffer = new TradeOffer(false, 0, string.Empty, new List<InventoryItem>());
            PlayerTwoOffer = new TradeOffer(false, 0, string.Empty, new List<InventoryItem>());
        }
    }
}