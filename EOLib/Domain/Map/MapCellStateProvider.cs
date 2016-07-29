// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using EOLib.Domain.Character;
using EOLib.IO.Map;
using EOLib.IO.Repositories;

namespace EOLib.Domain.Map
{
    public class MapCellStateProvider : IMapCellStateProvider
    {
        private readonly ICurrentMapStateProvider _mapStateProvider;
        private readonly IMapFileProvider _mapFileProvider;

        public MapCellStateProvider(ICurrentMapStateProvider mapStateProvider,
                                    IMapFileProvider mapFileProvider)
        {
            _mapStateProvider = mapStateProvider;
            _mapFileProvider = mapFileProvider;
        }

        public IMapCellState GetCellStateAt(int x, int y)
        {
            if (x < 0 || y < 0 || x > CurrentMap.Properties.Width || y > CurrentMap.Properties.Height)
                return new MapCellState {TileSpec = TileSpec.MapEdge};

            var tileSpec = CurrentMap.Tiles[y, x];
            var warp = CurrentMap.Warps[y, x];
            var chest = CurrentMap.Chests.FirstOrDefault(c => c.X == x && c.Y == y);
            var sign = CurrentMap.Signs.FirstOrDefault(s => s.X == x && s.Y == y);

            var character = _mapStateProvider.Characters.FirstOrDefault(c => c.RenderProperties.MapX == x &&
                                                                             c.RenderProperties.MapY == y);
            var npc = _mapStateProvider.NPCs.FirstOrDefault(n => n.X == x && n.Y == y);
            var items = _mapStateProvider.MapItems.Where(i => i.X == x && i.Y == y);

            return new MapCellState
            {
                Items = items.ToList(),
                TileSpec = tileSpec,
                Warp = new MapWarp(warp),
                Chest = new MapChest(chest),
                Sign = new MapSign(sign),
                Character = new Optional<ICharacter>(character),
                NPC = new Optional<IMapNPC>(npc)
            };
        }

        private IReadOnlyMapFile CurrentMap { get { return _mapFileProvider.MapFiles[_mapStateProvider.CurrentMapID]; } }
    }
}