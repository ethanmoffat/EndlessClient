using EOLib.Domain.Character;
using EOLib.IO.Map;
using Optional;
using System.Collections.Generic;

namespace EOLib.Domain.Map
{
    public class MapCellState : IMapCellState
    {
        public bool InBounds { get; set; }

        public MapCoordinate Coordinate { get; set; }

        public IReadOnlyList<IItem> Items { get; set; }

        public TileSpec TileSpec { get; set; }

        public Option<NPC.NPC> NPC { get; set; }

        public Option<Character.Character> Character { get; set; }

        public Option<ChestKey> ChestKey { get; set; }

        public Option<IWarp> Warp { get; set; }

        public Option<ISign> Sign { get; set; }

        public MapCellState()
        {
            Coordinate = new MapCoordinate(0, 0);
            Items = new List<IItem>();
            TileSpec = TileSpec.None;
            NPC = Option.None<NPC.NPC>();
            Character = Option.None<Character.Character>();
            ChestKey = Option.None<ChestKey>();
            Warp = Option.None<IWarp>();
            Sign = Option.None<ISign>();
        }
    }
}
