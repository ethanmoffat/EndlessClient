using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.NPC;
using EOLib.Extensions;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
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

        public IMapCellState GetCellStateAt(int x, int y)
        {
            if (x < 0 || y < 0 || x > CurrentMap.Properties.Width || y > CurrentMap.Properties.Height)
                return new MapCellState { InBounds = false, TileSpec = TileSpec.MapEdge };

            var tileSpec = CurrentMap.Tiles[y, x];
            var warp = CurrentMap.Warps[y, x];
            var chest = CurrentMap.Chests.FirstOrDefault(c => c.X == x && c.Y == y);
            var sign = CurrentMap.Signs.FirstOrDefault(s => s.X == x && s.Y == y);

            var character = _mapStateProvider.Characters.Values.Concat(new[] { _characterProvider.MainCharacter })
                .OptionalFirst(c => CharacterAtCoordinates(c, x, y));
            var npc = _mapStateProvider.NPCs.OptionalFirst(n => NPCAtCoordinates(n, x, y));
            var items = _mapStateProvider.MapItems.Where(i => i.X == x && i.Y == y);

            return new MapCellState
            {
                InBounds   = true,
                Coordinate = new MapCoordinate(x, y),
                Items      = items.ToList(),
                TileSpec   = tileSpec,
                Warp       = warp == null ? Optional<IWarp>.Empty : new Warp(warp),
                Chest      = chest == null ? Optional<IChest>.Empty : new Chest(chest),
                Sign       = sign == null ? Optional<ISign>.Empty : new Sign(sign),
                Character  = character,
                NPC        = npc
            };
        }

        private static bool CharacterAtCoordinates(ICharacter character, int x, int y)
        {
            return character.RenderProperties.IsActing(CharacterActionState.Walking)
                ? character.RenderProperties.GetDestinationX() == x && character.RenderProperties.GetDestinationY() == y
                : character.RenderProperties.MapX == x && character.RenderProperties.MapY == y;
        }

        private static bool NPCAtCoordinates(INPC npc, int x, int y)
        {
            return npc.IsActing(NPCActionState.Walking)
                ? npc.GetDestinationX() == x && npc.GetDestinationY() == y
                : npc.X == x && npc.Y == y;
        }

        private IMapFile CurrentMap => _mapFileProvider.MapFiles[_mapStateProvider.CurrentMapID];
    }

    public interface IMapCellStateProvider
    {
        IMapCellState GetCellStateAt(int x, int y);
    }
}