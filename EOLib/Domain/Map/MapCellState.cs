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
        public IReadOnlyList<IItem> Items { get; set; }

        public TileSpec TileSpec { get; set; }

        public Optional<INPC> NPC { get; set; }

        public Optional<ICharacter> Character { get; set; }

        public Optional<IChest> Chest { get; set; }

        public Optional<IWarp> Warp { get; set; }

        public Optional<ISign> Sign { get; set; }

        public MapCellState()
        {
            Items = new List<IItem>();
            TileSpec = TileSpec.None;
            NPC = new Optional<INPC>();
            Character = new Optional<ICharacter>();
            Chest = new Optional<IChest>();
            Warp = new Optional<IWarp>();
            Sign = new Optional<ISign>();
        }
    }
}
