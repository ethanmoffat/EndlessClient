using EOLib.Domain.Character;
using EOLib.Net.Translators;
using System.Collections.Generic;

namespace EOLib.Domain.Map
{
    public class WarpAgreePacketData : IWarpAgreePacketData
    {
        public short MapID { get; private set; }

        public WarpAnimation WarpAnimation { get; private set; }

        public IReadOnlyList<ICharacter> Characters { get; private set; }

        public IReadOnlyList<NPC.NPC> NPCs { get; private set; }

        public IReadOnlyList<IItem> Items { get; private set; }

        public WarpAgreePacketData()
        {
            Characters = new List<ICharacter>();
            NPCs = new List<NPC.NPC>();
            Items = new List<IItem>();
        }

        public IWarpAgreePacketData WithMapID(short mapID)
        {
            var newData = MakeCopy(this);
            newData.MapID = mapID;
            return newData;
        }

        public IWarpAgreePacketData WithWarpAnimation(WarpAnimation warpAnimation)
        {
            var newData = MakeCopy(this);
            newData.WarpAnimation = warpAnimation;
            return newData;
        }

        public IWarpAgreePacketData WithCharacters(IEnumerable<ICharacter> characters)
        {
            var newData = MakeCopy(this);
            newData.Characters = new List<ICharacter>(characters);
            return newData;
        }

        public IWarpAgreePacketData WithNPCs(IEnumerable<NPC.NPC> npcs)
        {
            var newData = MakeCopy(this);
            newData.NPCs = new List<NPC.NPC>(npcs);
            return newData;
        }

        public IWarpAgreePacketData WithItems(IEnumerable<IItem> items)
        {
            var newData = MakeCopy(this);
            newData.Items = new List<IItem>(items);
            return newData;
        }

        private static WarpAgreePacketData MakeCopy(IWarpAgreePacketData source)
        {
            return new WarpAgreePacketData
            {
                MapID = source.MapID,
                WarpAnimation = source.WarpAnimation,
                Characters = new List<ICharacter>(source.Characters),
                NPCs = new List<NPC.NPC>(source.NPCs),
                Items = new List<IItem>(source.Items)
            };
        }
    }

    public interface IWarpAgreePacketData : ITranslatedData
    {
        short MapID { get; }

        WarpAnimation WarpAnimation { get; }

        IReadOnlyList<ICharacter> Characters { get; }

        IReadOnlyList<NPC.NPC> NPCs { get; }

        IReadOnlyList<IItem> Items { get; }

        IWarpAgreePacketData WithMapID(short mapID);

        IWarpAgreePacketData WithWarpAnimation(WarpAnimation warpAnimation);

        IWarpAgreePacketData WithCharacters(IEnumerable<ICharacter> characters);

        IWarpAgreePacketData WithNPCs(IEnumerable<NPC.NPC> npcs);

        IWarpAgreePacketData WithItems(IEnumerable<IItem> items);
    }
}
