using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Domain.NPC;
using EOLib.IO.Map;
using Optional;

namespace EOLib.Domain.Map
{
    public class MapCellState : IMapCellState
    {
        public bool InBounds { get; set; }

        public MapCoordinate Coordinate { get; set; }

        public IReadOnlyList<IItem> Items { get; set; }

        public TileSpec TileSpec { get; set; }

        public Option<INPC> NPC { get; set; }

        public Option<ICharacter> Character { get; set; }

        public Option<IChest> Chest { get; set; }

        public Option<IWarp> Warp { get; set; }

        public Option<ISign> Sign { get; set; }

        public MapCellState()
        {
            Coordinate = new MapCoordinate(0, 0);
            Items = new List<IItem>();
            TileSpec = TileSpec.None;
            NPC = Option.None<INPC>();
            Character = Option.None<ICharacter>();
            Chest = Option.None<IChest>();
            Warp = Option.None<IWarp>();
            Sign = Option.None<ISign>();
        }
    }
}
