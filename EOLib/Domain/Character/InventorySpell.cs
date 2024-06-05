using Amadevus.RecordGenerator;
using Moffat.EndlessOnline.SDK.Protocol.Net;

namespace EOLib.Domain.Character
{
    [Record]
    public sealed partial class InventorySpell
    {
        public int ID { get; }

        public int Level { get; }

        public static InventorySpell FromNet(Spell source) => new InventorySpell(source.Id, source.Level);
    }
}