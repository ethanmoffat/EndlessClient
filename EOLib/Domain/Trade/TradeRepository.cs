using EOLib.Domain.Character;
using System.Collections.Generic;

namespace EOLib.Domain.Trade
{
    public interface ITradeRepository : IResettable
    {
        byte TradeSessionID { get; set; }

        TradeOffer PlayerOneOffer { get; set; }

        TradeOffer PlayerTwoOffer { get; set; }
    }

    public interface ITradeProvider
    {
        byte TradeSessionID { get; }

        TradeOffer PlayerOneOffer { get; }

        TradeOffer PlayerTwoOffer { get; }
    }

    public class TradeRepository : ITradeRepository, ITradeProvider
    {
        public byte TradeSessionID { get; set; }

        public TradeOffer PlayerOneOffer { get; set; }

        public TradeOffer PlayerTwoOffer { get; set; }

        public TradeRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            TradeSessionID = 0;
            PlayerOneOffer = new TradeOffer(false, 0, string.Empty, new List<InventoryItem>());
            PlayerTwoOffer = new TradeOffer(false, 0, string.Empty, new List<InventoryItem>());
        }
    }
}
