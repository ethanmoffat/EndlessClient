using Amadevus.RecordGenerator;
using System;

using NetItem = Moffat.EndlessOnline.SDK.Protocol.Net.Item;

namespace EOLib.Domain.Character
{
    [Record(Features.Default | Features.EquatableEquals | Features.ObjectEquals)]
    public sealed partial class InventoryItem
    {
        public int ItemID { get; }

        public int Amount { get; }

        public static InventoryItem FromNet(NetItem source) => new InventoryItem(source.Id, source.Amount);
    }
}