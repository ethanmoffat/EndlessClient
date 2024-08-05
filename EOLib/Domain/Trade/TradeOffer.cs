using System.Collections.Generic;
using Amadevus.RecordGenerator;
using EOLib.Domain.Character;

namespace EOLib.Domain.Trade
{
    [Record(Features.Default | Features.EquatableEquals | Features.ObjectEquals)]
    public sealed partial class TradeOffer
    {
        public bool Agrees { get; }

        public int PlayerID { get; }

        public string PlayerName { get; }

        public IReadOnlyList<InventoryItem> Items { get; }
    }
}
