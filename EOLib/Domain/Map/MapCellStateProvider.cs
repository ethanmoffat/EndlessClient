using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.NPC;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Optional;
using Optional.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.Domain.Map
{
    [AutoMappedType]
    public class MapCellStateProvider : IMapCellStateProvider
    {
        private readonly ICurrentMapStateProvider _mapStateProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IMapFileProvider _mapFileProvider;

        public MapCellStateProvider(ICurrentMapStateProvider mapStateProvider,
                                    ICharacterProvider characterProvider,
                                    IMapFileProvider mapFileProvider)
        {
            _mapStateProvider = mapStateProvider;
            _characterProvider = characterProvider;
            _mapFileProvider = mapFileProvider;
        }

        public IMapCellState GetCellStateAt(MapCoordinate mapCoordinate) => GetCellStateAt(mapCoordinate.X, mapCoordinate.Y);

        public IMapCellState GetCellStateAt(int x, int y)
        {
            if (x < 0 || y < 0 || x > CurrentMap.Properties.Width || y > CurrentMap.Properties.Height)
                return new MapCellState { InBounds = false, TileSpec = TileSpec.MapEdge };

            var tileSpec = CurrentMap.Tiles[y, x];
            var warp = CurrentMap.Warps[y, x];
            var chest = CurrentMap.Chests.Where(c => c.X == x && c.Y == y && c.Key != ChestKey.None).Select(c => c.Key).FirstOrDefault();
            var sign = CurrentMap.Signs.FirstOrDefault(s => s.X == x && s.Y == y);

            _mapStateProvider.Characters.TryGetValues(new MapCoordinate(x, y), out var characters);
            if (_characterProvider.MainCharacter.X == x && _characterProvider.MainCharacter.Y == y)
                characters.Add(_characterProvider.MainCharacter);

            Option<NPC.NPC> npc = Option.None<NPC.NPC>();
            if (_mapStateProvider.NPCs.TryGetValues(new MapCoordinate(x, y), out var npcs))
                npc = npcs.FirstOrNone();

            var items = _mapStateProvider.MapItems.TryGetValues(new MapCoordinate(x, y), out var mapItems)
                ? mapItems.OrderByDescending(i => i.UniqueID)
                : Enumerable.Empty<MapItem>();

            return new MapCellState
            {
                InBounds   = true,
                Coordinate = new MapCoordinate(x, y),
                Items      = items.ToList(),
                TileSpec   = tileSpec,
                Warp       = warp.SomeNotNull().Map(w => new Warp(w)),
                ChestKey   = chest.SomeNotNull(),
                Sign       = sign.SomeNotNull().Map(s => new Sign(s)),
                Character  = characters.FirstOrNone(),
                Characters = characters.ToList(),
                NPC        = npc
            };
        }

        private IMapFile CurrentMap => _mapFileProvider.MapFiles[_mapStateProvider.CurrentMapID];
    }

    public interface IMapCellStateProvider
    {
        IMapCellState GetCellStateAt(MapCoordinate mapCoordinate);

        IMapCellState GetCellStateAt(int x, int y);
    }
}