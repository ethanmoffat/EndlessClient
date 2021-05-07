using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class RoofLayerRenderer : BaseMapEntityRenderer
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICurrentMapProvider _currentMapProvider;

        public override MapRenderLayer RenderLayer => MapRenderLayer.Roof;

        protected override int RenderDistance => 12;

        public RoofLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                 ICurrentMapProvider currentMapProvider,
                                 ICharacterProvider characterProvider,
                                 IRenderOffsetCalculator renderOffsetCalculator,
                                 IClientWindowSizeProvider clientWindowSizeProvider)
            : base(characterProvider, renderOffsetCalculator, clientWindowSizeProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _currentMapProvider = currentMapProvider;
        }

        protected override bool ElementExistsAt(int row, int col)
        {
            return CurrentMap.GFX[MapLayer.Roof][row, col] > 0;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            int gfxNum = CurrentMap.GFX[MapLayer.Roof][row, col];
            var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapWallTop, gfxNum, true);

            var pos = GetDrawCoordinatesFromGridUnits(col, row);
            spriteBatch.Draw(gfx, pos + additionalOffset, Color.FromNonPremultiplied(255, 255, 255, alpha));
        }

        private IMapFile CurrentMap => _currentMapProvider.CurrentMap;
    }
}
