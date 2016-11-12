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

        public IReadOnlyList<INPC> NPCs { get; private set; }

        public IReadOnlyList<IItem> MapItems { get; private set; }

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

        public IWarpAgreePacketData WithNPCs(IEnumerable<INPC> npcs)
        {
            var newData = MakeCopy(this);
            newData.NPCs = new List<INPC>(npcs);
            return newData;
        }

        public IWarpAgreePacketData WithItems(IEnumerable<IItem> items)
        {
            var newData = MakeCopy(this);
            newData.MapItems = new List<IItem>(items);
            return newData;
        }

        private static WarpAgreePacketData MakeCopy(IWarpAgreePacketData source)
        {
            return new WarpAgreePacketData
            {
                MapID = source.MapID,
                WarpAnimation = source.WarpAnimation,
                Characters = new List<ICharacter>(source.Characters),
                NPCs = new List<INPC>(source.NPCs),
                MapItems = new List<IItem>(source.MapItems)
            };
        }
    }

    public interface IWarpAgreePacketData : ITranslatedData
    {
        short MapID { get; }

        WarpAnimation WarpAnimation { get; }

        IReadOnlyList<ICharacter> Characters { get; }

        IReadOnlyList<INPC> NPCs { get; }

        IReadOnlyList<IItem> MapItems { get; }

        IWarpAgreePacketData WithMapID(short mapID);

        IWarpAgreePacketData WithWarpAnimation(WarpAnimation warpAnimation);

        IWarpAgreePacketData WithCharacters(IEnumerable<ICharacter> characters);

        IWarpAgreePacketData WithNPCs(IEnumerable<INPC> npcs);

        IWarpAgreePacketData WithItems(IEnumerable<IItem> items);
    }
}
