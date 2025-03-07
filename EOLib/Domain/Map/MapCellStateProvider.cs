using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.NPC;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Optional;
using Optional.Collections;

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
            if (x <= 0 || y <= 0 || x > CurrentMap.Properties.Width || y > CurrentMap.Properties.Height)
                return new MapCellState { InBounds = false, TileSpec = TileSpec.MapEdge };

            var tileSpec = CurrentMap.Tiles[y, x];
            var warp = CurrentMap.Warps[y, x];
            var chest = CurrentMap.Chests.Where(c => c.X == x && c.Y == y && c.Key != ChestKey.None).Select(c => c.Key).FirstOrDefault();
            var sign = CurrentMap.Signs.FirstOrDefault(s => s.X == x && s.Y == y);

            var characters = new List<Character.Character>();
            if (_mapStateProvider.Characters.ContainsKey(new MapCoordinate(x, y)))
                characters = _mapStateProvider.Characters[new MapCoordinate(x, y)].ToList();
            if (_characterProvider.MainCharacter.X == x && _characterProvider.MainCharacter.Y == y)
                characters.Add(_characterProvider.MainCharacter);

            Option<NPC.NPC> npc = Option.None<NPC.NPC>();
            if (_mapStateProvider.NPCs.ContainsKey(new MapCoordinate(x, y)))
            {
                npc = _mapStateProvider.NPCs[new MapCoordinate(x, y)].FirstOrNone();
                if (npc.Map(x => x.IsActing(NPCActionState.Walking)).ValueOr(false))
                    npc = Option.None<NPC.NPC>();
            }

            var items = new List<MapItem>();
            if (_mapStateProvider.MapItems.ContainsKey(new MapCoordinate(x, y)))
                items = _mapStateProvider.MapItems[new MapCoordinate(x, y)].OrderByDescending(i => i.UniqueID).ToList();

            return new MapCellState
            {
                InBounds = true,
                Coordinate = new MapCoordinate(x, y),
                Items = items,
                TileSpec = tileSpec,
                Warp = warp.SomeNotNull().Map(w => new Warp(w)),
                ChestKey = chest.SomeNotNull(),
                Sign = sign.SomeNotNull().Map(s => new Sign(s)),
                Character = characters.FirstOrNone(),
                Characters = characters,
                NPC = npc
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
