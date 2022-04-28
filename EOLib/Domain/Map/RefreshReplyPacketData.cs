using Amadevus.RecordGenerator;
using EOLib.Net.Translators;
using System.Collections.Generic;

namespace EOLib.Domain.Map
{
    [Record]
    public sealed partial class RefreshReplyData : ITranslatedData
    {
        public IReadOnlyList<Character.Character> Characters { get; }

        public IReadOnlyList<NPC.NPC> NPCs { get; }

        public IReadOnlyList<MapItem> Items { get; }

        public RefreshReplyData()
        {
            Characters = new List<Character.Character>();
            NPCs = new List<NPC.NPC>();
            Items = new List<MapItem>();
        }
    }
}
