using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Domain.NPC;
using EOLib.IO.Map;

namespace EOLib.Domain.Map
{
    public interface IMapCellState
    {
        IReadOnlyList<IItem> Items { get; }

        TileSpec TileSpec { get; }

        Optional<INPC> NPC { get; }

        Optional<ICharacter> Character { get; }
        
        Optional<IChest> Chest { get; }

        Optional<IWarp> Warp { get; }

        Optional<ISign> Sign { get; }
    }
}
