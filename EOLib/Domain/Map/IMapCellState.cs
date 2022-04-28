using EOLib.Domain.Character;
using EOLib.IO.Map;
using Optional;
using System.Collections.Generic;

namespace EOLib.Domain.Map
{
    public interface IMapCellState
    {
        bool InBounds { get; }

        MapCoordinate Coordinate { get;  }

        IReadOnlyList<MapItem> Items { get; }

        TileSpec TileSpec { get; }

        Option<NPC.NPC> NPC { get; }

        Option<Character.Character> Character { get; }
        
        Option<ChestKey> ChestKey { get; }

        Option<Warp> Warp { get; }

        Option<Sign> Sign { get; }
    }
}
