// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.IO.Map;

namespace EOLib.Domain.Map
{
    public class MapCellState : IMapCellState
    {
        public IReadOnlyList<IMapItem> Items { get; set; }

        public TileSpec TileSpec { get; set; }

        public Optional<IMapNPC> NPC { get; set; }

        public Optional<ICharacter> Character { get; set; }

        public Optional<IMapChest> Chest { get; set; }

        public Optional<IMapWarp> Warp { get; set; }

        public Optional<IMapSign> Sign { get; set; }

        public MapCellState()
        {
            Items = new List<IMapItem>();
            TileSpec = TileSpec.None;
            NPC = new Optional<IMapNPC>();
            Character = new Optional<ICharacter>();
            Chest = new Optional<IMapChest>();
            Warp = new Optional<IMapWarp>();
            Sign = new Optional<IMapSign>();
        }
    }
}
