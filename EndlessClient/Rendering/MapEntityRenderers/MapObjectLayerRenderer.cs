using System;
using System.Linq;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class MapObjectLayerRenderer : BaseMapEntityRenderer
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;

        public override MapRenderLayer RenderLayer => MapRenderLayer.Objects;

        protected override int RenderDistance => 22;

        public MapObjectLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                      ICurrentMapProvider currentMapProvider,
                                      ICharacterProvider characterProvider,
                                      IRenderOffsetCalculator renderOffsetCalculator,
                                      IClientWindowSizeProvider clientWindowSizeProvider,
                                      ICurrentMapStateProvider currentMapStateProvider)
            : base(characterProvider, renderOffsetCalculator, clientWindowSizeProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _currentMapProvider = currentMapProvider;
            _currentMapStateProvider = currentMapStateProvider;
        }

        protected override bool ElementExistsAt(int row, int col)
        {
            return MapFile.GFX[MapLayer.Objects][row, col] > 0 &&
                (MapFile.Tiles[row, col] != TileSpec.SpikesTrap ||
                _currentMapStateProvider.VisibleSpikeTraps.Contains(new MapCoordinate(col, row)));
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            int gfxNum = MapFile.GFX[MapLayer.Objects][row, col];
            var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapObjects, gfxNum, true);

            var pos = GetDrawCoordinatesFromGridUnits(col, row);
            pos -= new Vector2(gfx.Width / 2, gfx.Height - 32);

            spriteBatch.Draw(gfx, pos + additionalOffset, Color.FromNonPremultiplied(255, 255, 255, alpha));
        }

        private IMapFile MapFile => _currentMapProvider.CurrentMap;
    }
}
