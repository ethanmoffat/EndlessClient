namespace EOLib.Domain.Trade
{
    public interface ITradeRepository : IResettable
    {
        byte TradeSessionID { get; set; }
    }

    public interface ITradeProvider
    {
        byte TradeSessionID { get; }
    }

    public class TradeRepository : ITradeRepository, ITradeProvider
    {
        public byte TradeSessionID { get; set; }

        public TradeRepository()
        {
            ResetState();
        }

        public void ResetState()
        {
            TradeSessionID = 0;
        }
    }
}
