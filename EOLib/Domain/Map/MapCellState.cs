using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Domain.NPC;
using EOLib.IO.Map;

namespace EOLib.Domain.Map
{
    public class MapCellState : IMapCellState
    {
        public MapCoordinate Coordinate { get; set; }

        public IReadOnlyList<IItem> Items { get; set; }

        public TileSpec TileSpec { get; set; }

        public Optional<INPC> NPC { get; set; }

        public Optional<ICharacter> Character { get; set; }

        public Optional<IChest> Chest { get; set; }

        public Optional<IWarp> Warp { get; set; }

        public Optional<ISign> Sign { get; set; }

        public MapCellState()
        {
            Coordinate = new MapCoordinate(0, 0);
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
