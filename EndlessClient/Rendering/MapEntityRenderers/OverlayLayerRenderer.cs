using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class OverlayLayerRenderer : BaseMapEntityRenderer
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICurrentMapProvider _currentMapProvider;

        public override MapRenderLayer RenderLayer => MapRenderLayer.Overlay;

        protected override int RenderDistance => 14;

        public OverlayLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                    ICurrentMapProvider currentMapProvider,
                                    ICharacterProvider characterProvider,
                                    IGridDrawCoordinateCalculator gridDrawCoordinateCalculator)
            : base(characterProvider, gridDrawCoordinateCalculator)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _currentMapProvider = currentMapProvider;
        }

        protected override bool ElementExistsAt(int row, int col)
        {
            return CurrentMap.GFX[MapLayer.OverlayObjects][row, col] > 0;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            int gfxNum = CurrentMap.GFX[MapLayer.OverlayObjects][row, col];
            var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapOverlay, gfxNum, true);

            var pos = GetDrawCoordinatesFromGridUnits(col, row);
            pos -= new Vector2(gfx.Width / 2, gfx.Height - 32);

            spriteBatch.Draw(gfx, pos + additionalOffset, Color.FromNonPremultiplied(255, 255, 255, alpha));
        }

        private IMapFile CurrentMap => _currentMapProvider.CurrentMap;
    }
}
