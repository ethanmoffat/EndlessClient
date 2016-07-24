// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Domain.NPC;
using EOLib.IO.Map;

namespace EOLib.Domain.Map
{
    public interface IMapCellState
    {
        TileInfoReturnType ReturnType { get; }

        IReadOnlyList<IMapItem> Items { get; }

        TileSpec TileSpec { get; }

        Optional<IMapNPC> NPC { get; }

        Optional<ICharacter> Character { get; }
        
        Optional<IMapChest> Chest { get; }

        Optional<IMapWarp> Warp { get; }

        Optional<IMapSign> Sign { get; }
    }
}
