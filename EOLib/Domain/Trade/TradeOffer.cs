using Amadevus.RecordGenerator;
using EOLib.Domain.Character;
using System.Collections.Generic;

namespace EOLib.Domain.Trade
{
    [Record(Features.Default | Features.EquatableEquals | Features.ObjectEquals)]
    public sealed partial class TradeOffer
    {
        public bool Agrees { get; }

        public short PlayerID { get; }

        public string PlayerName { get; }

        public IReadOnlyList<InventoryItem> Items { get; }
    }
}
