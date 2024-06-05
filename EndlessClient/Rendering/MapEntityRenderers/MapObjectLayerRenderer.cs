using System;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using DomainCharacter = EOLib.Domain.Character.Character;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class MapObjectLayerRenderer : BaseMapEntityRenderer
    {
        private const int TIMED_SPIKE_DURATION_MS = 1000;

        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;

        public override MapRenderLayer RenderLayer => MapRenderLayer.Objects;

        protected override int RenderDistance => 22;

        public MapObjectLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                      ICurrentMapProvider currentMapProvider,
                                      ICharacterProvider characterProvider,
                                      IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                      IClientWindowSizeProvider clientWindowSizeProvider,
                                      ICurrentMapStateProvider currentMapStateProvider)
            : base(characterProvider, gridDrawCoordinateCalculator, clientWindowSizeProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _currentMapProvider = currentMapProvider;
            _currentMapStateProvider = currentMapStateProvider;
        }

        protected override bool ElementExistsAt(int row, int col)
        {
            return MapFile.GFX[MapLayer.Objects][row, col] > 0;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            if (MapFile.Tiles[row, col] == TileSpec.SpikesTrap)
            {
                var loc = new MapCoordinate(col, row);
                var mainCharacterAt = CharacterAt(_characterProvider.MainCharacter, loc);
                if (!mainCharacterAt && _currentMapStateProvider.Characters.ContainsKey(loc))
                {
                    var anyOtherCharactersAt = _currentMapStateProvider.Characters[loc].Any(x => CharacterAt(x, loc));
                    if (!anyOtherCharactersAt)
                    {
                        return;
                    }
                }
                else if (!mainCharacterAt)
                {
                    return;
                }
            }
            else if (MapFile.Tiles[row, col] == TileSpec.SpikesTimed)
            {
                var shouldRender = _currentMapStateProvider.LastTimedSpikeEvent
                    .Map(time => (DateTime.Now - time).TotalMilliseconds <= TIMED_SPIKE_DURATION_MS)
                    .ValueOr(false);
                if (!shouldRender)
                    return;
            }

            int gfxNum = MapFile.GFX[MapLayer.Objects][row, col];
            var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapObjects, gfxNum, true);

            var pos = GetDrawCoordinatesFromGridUnits(col, row);
            pos -= new Vector2(gfx.Width / 2, gfx.Height - 32);

            spriteBatch.Draw(gfx, pos + additionalOffset, Color.FromNonPremultiplied(255, 255, 255, alpha));
        }

        private IMapFile MapFile => _currentMapProvider.CurrentMap;

        private static bool CharacterAt(DomainCharacter c, MapCoordinate tile)
        {
            return c.RenderProperties.CurrentAction != CharacterActionState.Walking
                ? tile == c.RenderProperties.Coordinates()
                : tile == c.RenderProperties.DestinationCoordinates();
        }
    }
}
