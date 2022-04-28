using EOLib.Net.Translators;
using System.Collections.Generic;

namespace EOLib.Domain.Map
{
    public class RefreshReplyData : IRefreshReplyData
    {
        public IReadOnlyList<Character.Character> Characters { get; private set; }

        public IReadOnlyList<NPC.NPC> NPCs { get; private set; }

        public IReadOnlyList<MapItem> Items { get; private set; }

        public RefreshReplyData()
        {
            Characters = new List<Character.Character>();
            NPCs = new List<NPC.NPC>();
            Items = new List<MapItem>();
        }

        public IRefreshReplyData WithCharacters(IEnumerable<Character.Character> characters)
        {
            var newData = MakeCopy(this);
            newData.Characters = new List<Character.Character>(characters);
            return newData;
        }

        public IRefreshReplyData WithNPCs(IEnumerable<NPC.NPC> npcs)
        {
            var newData = MakeCopy(this);
            newData.NPCs = new List<NPC.NPC>(npcs);
            return newData;
        }

        public IRefreshReplyData WithItems(IEnumerable<MapItem> items)
        {
            var newData = MakeCopy(this);
            newData.Items = new List<MapItem>(items);
            return newData;
        }

        private static RefreshReplyData MakeCopy(IRefreshReplyData source)
        {
            return new RefreshReplyData
            {
                Characters = new List<Character.Character>(source.Characters),
                NPCs = new List<NPC.NPC>(source.NPCs),
                Items = new List<MapItem>(source.Items)
            };
        }
    }

    public interface IRefreshReplyData : ITranslatedData
    {
        IReadOnlyList<Character.Character> Characters { get; }

        IReadOnlyList<NPC.NPC> NPCs { get; }

        IReadOnlyList<MapItem> Items { get; }

        IRefreshReplyData WithCharacters(IEnumerable<Character.Character> characters);

        IRefreshReplyData WithNPCs(IEnumerable<NPC.NPC> npcs);

        IRefreshReplyData WithItems(IEnumerable<MapItem> items);
    }
}
