using EOLib.Domain.Character;
using EOLib.Net.Translators;
using System.Collections.Generic;

namespace EOLib.Domain.Map
{
    public class RefreshReplyData : IRefreshReplyData
    {
        public IReadOnlyList<ICharacter> Characters { get; private set; }

        public IReadOnlyList<NPC.NPC> NPCs { get; private set; }

        public IReadOnlyList<IItem> Items { get; private set; }

        public RefreshReplyData()
        {
            Characters = new List<ICharacter>();
            NPCs = new List<NPC.NPC>();
            Items = new List<IItem>();
        }

        public IRefreshReplyData WithCharacters(IEnumerable<ICharacter> characters)
        {
            var newData = MakeCopy(this);
            newData.Characters = new List<ICharacter>(characters);
            return newData;
        }

        public IRefreshReplyData WithNPCs(IEnumerable<NPC.NPC> npcs)
        {
            var newData = MakeCopy(this);
            newData.NPCs = new List<NPC.NPC>(npcs);
            return newData;
        }

        public IRefreshReplyData WithItems(IEnumerable<IItem> items)
        {
            var newData = MakeCopy(this);
            newData.Items = new List<IItem>(items);
            return newData;
        }

        private static RefreshReplyData MakeCopy(IRefreshReplyData source)
        {
            return new RefreshReplyData
            {
                Characters = new List<ICharacter>(source.Characters),
                NPCs = new List<NPC.NPC>(source.NPCs),
                Items = new List<IItem>(source.Items)
            };
        }
    }

    public interface IRefreshReplyData : ITranslatedData
    {
        IReadOnlyList<ICharacter> Characters { get; }

        IReadOnlyList<NPC.NPC> NPCs { get; }

        IReadOnlyList<IItem> Items { get; }

        IRefreshReplyData WithCharacters(IEnumerable<ICharacter> characters);

        IRefreshReplyData WithNPCs(IEnumerable<NPC.NPC> npcs);

        IRefreshReplyData WithItems(IEnumerable<IItem> items);
    }
}
