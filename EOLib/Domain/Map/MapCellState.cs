using System.Collections.Generic;
using EOLib.IO.Map;
using Optional;

namespace EOLib.Domain.Map
{
    public class MapCellState : IMapCellState
    {
        public bool InBounds { get; set; }

        public MapCoordinate Coordinate { get; set; }

        public IReadOnlyList<MapItem> Items { get; set; }

        public TileSpec TileSpec { get; set; }

        public Option<NPC.NPC> NPC { get; set; }

        public Option<Character.Character> Character { get; set; }

        public IReadOnlyList<Character.Character> Characters { get; set; }

        public Option<ChestKey> ChestKey { get; set; }

        public Option<Warp> Warp { get; set; }

        public Option<Sign> Sign { get; set; }

        public MapCellState()
        {
            Coordinate = new MapCoordinate(0, 0);
            Items = new List<MapItem>();
            TileSpec = TileSpec.None;
            NPC = Option.None<NPC.NPC>();
            Character = Option.None<Character.Character>();
            Characters = new List<Character.Character>();
            ChestKey = Option.None<ChestKey>();
            Warp = Option.None<Warp>();
            Sign = Option.None<Sign>();
        }
    }
}
