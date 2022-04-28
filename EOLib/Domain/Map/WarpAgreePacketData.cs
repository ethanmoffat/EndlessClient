using Amadevus.RecordGenerator;
using EOLib.Net.Translators;
using System.Collections.Generic;

namespace EOLib.Domain.Map
{
    [Record]
    public sealed partial class WarpAgreePacketData : ITranslatedData
    {
        public short MapID { get; }

        public WarpAnimation WarpAnimation { get; }

        public IReadOnlyList<Character.Character> Characters { get; }

        public IReadOnlyList<NPC.NPC> NPCs { get; }

        public IReadOnlyList<MapItem> Items { get; }

        public WarpAgreePacketData()
        {
            Characters = new List<Character.Character>();
            NPCs = new List<NPC.NPC>();
            Items = new List<MapItem>();
        }
    }
}
