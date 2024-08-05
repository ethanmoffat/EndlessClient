using EndlessClient.Rendering.Map;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class ShadowLayerRenderer : BaseMapEntityRenderer
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IConfigurationProvider _configurationProvider;

        public override MapRenderLayer RenderLayer => MapRenderLayer.Shadows;

        protected override int RenderDistance => 16;

        public ShadowLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                   ICurrentMapProvider currentMapProvider,
                                   ICharacterProvider characterProvider,
                                   IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                   IClientWindowSizeProvider clientWindowSizeProvider,
                                   IConfigurationProvider configurationProvider)
            : base(characterProvider, gridDrawCoordinateCalculator, clientWindowSizeProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _currentMapProvider = currentMapProvider;
            _configurationProvider = configurationProvider;
        }

        public override bool CanRender(int row, int col)
        {
            return _configurationProvider.ShowShadows && base.CanRender(row, col);
        }

        protected override bool ElementExistsAt(int row, int col)
        {
            return CurrentMap.GFX[MapLayer.Shadow][row, col] > 0;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            int gfxNum = CurrentMap.GFX[MapLayer.Shadow][row, col];
            var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.Shadows, gfxNum, true);

            var pos = GetDrawCoordinatesFromGridUnits(col, row);
            pos = new Vector2(pos.X - 32, pos.Y);

            spriteBatch.Draw(gfx, pos + additionalOffset, Color.FromNonPremultiplied(255, 255, 255, 255 / 5));
        }

        private IMapFile CurrentMap => _currentMapProvider.CurrentMap;
    }
}