using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Domain.NPC;
using EOLib.IO.Map;
using Optional;

namespace EOLib.Domain.Map
{
    public interface IMapCellState
    {
        bool InBounds { get; }

        MapCoordinate Coordinate { get;  }

        IReadOnlyList<IItem> Items { get; }

        TileSpec TileSpec { get; }

        Option<INPC> NPC { get; }

        Option<ICharacter> Character { get; }
        
        Option<IChest> Chest { get; }

        Option<IWarp> Warp { get; }

        Option<ISign> Sign { get; }
    }
}
