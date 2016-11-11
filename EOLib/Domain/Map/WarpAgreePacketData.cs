// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Net.Translators;

namespace EOLib.Domain.Map
{
    public class WarpAgreePacketData : IWarpAgreePacketData
    {
        public short MapID { get; private set; }

        public WarpAnimation WarpAnimation { get; private set; }

        public IReadOnlyList<ICharacter> Characters { get; private set; }

        public IReadOnlyList<IMapNPC> NPCs { get; private set; }

        public IReadOnlyList<IMapItem> MapItems { get; private set; }

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

        public IWarpAgreePacketData WithNPCs(IEnumerable<IMapNPC> npcs)
        {
            var newData = MakeCopy(this);
            newData.NPCs = new List<IMapNPC>(npcs);
            return newData;
        }

        public IWarpAgreePacketData WithItems(IEnumerable<IMapItem> items)
        {
            var newData = MakeCopy(this);
            newData.MapItems = new List<IMapItem>(items);
            return newData;
        }

        private static WarpAgreePacketData MakeCopy(IWarpAgreePacketData source)
        {
            return new WarpAgreePacketData
            {
                MapID = source.MapID,
                WarpAnimation = source.WarpAnimation,
                Characters = new List<ICharacter>(source.Characters),
                NPCs = new List<IMapNPC>(source.NPCs),
                MapItems = new List<IMapItem>(source.MapItems)
            };
        }
    }

    public interface IWarpAgreePacketData : ITranslatedData
    {
        short MapID { get; }

        WarpAnimation WarpAnimation { get; }

        IReadOnlyList<ICharacter> Characters { get; }

        IReadOnlyList<IMapNPC> NPCs { get; }

        IReadOnlyList<IMapItem> MapItems { get; }

        IWarpAgreePacketData WithMapID(short mapID);

        IWarpAgreePacketData WithWarpAnimation(WarpAnimation warpAnimation);

        IWarpAgreePacketData WithCharacters(IEnumerable<ICharacter> characters);

        IWarpAgreePacketData WithNPCs(IEnumerable<IMapNPC> npcs);

        IWarpAgreePacketData WithItems(IEnumerable<IMapItem> items);
    }
}
