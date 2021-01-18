using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Domain.Map;
using EOLib.Graphics;
using EOLib.IO.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class GroundLayerRenderer : BaseMapEntityRenderer
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICurrentMapProvider _currentMapProvider;

        public override MapRenderLayer RenderLayer => MapRenderLayer.Ground;

        protected override int RenderDistance => 10;

        public GroundLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                   ICurrentMapProvider currentMapProvider,
                                   ICharacterProvider characterProvider,
                                   IRenderOffsetCalculator renderOffsetCalculator)
            : base(characterProvider, renderOffsetCalculator)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _currentMapProvider = currentMapProvider;
        }

        protected override Vector2 GetDrawCoordinatesFromGridUnits(int gridX, int gridY)
        {
            // the height is used to offset the 0 point of the grid, which is 32 units per tile in the height of the map
            var height = _currentMapProvider.CurrentMap.Properties.Height;
            return new Vector2((gridX * 32) - (gridY * 32) + (32*height),
                               (gridY * 16) + (gridX * 16));
        }

        public override bool CanRender(int row, int col) => true;

        protected override bool ElementExistsAt(int row, int col)
        {
            return (CurrentMap.Properties.FillTile > 0 && CurrentMap.GFX[MapLayer.GroundTile][row, col] < 0) ||
                   CurrentMap.GFX[MapLayer.GroundTile][row, col] > 0;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
        {
            var pos = GetDrawCoordinatesFromGridUnits(col, row);

            int tileGFX;
            if ((tileGFX = CurrentMap.Properties.FillTile) > 0 && CurrentMap.GFX[MapLayer.GroundTile][row, col] < 0)
            {
                //todo: source rectangle for fill tile
                var fillTile = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapTiles, tileGFX, true);
                spriteBatch.Draw(fillTile, pos, Color.FromNonPremultiplied(255, 255, 255, alpha));
            }
            else if ((tileGFX = CurrentMap.GFX[MapLayer.GroundTile][row, col]) > 0)
            {
                var tile = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapTiles, tileGFX, true);

                //todo: animate ground tiles by adjusting the source rectangle offset
                var src = tile.Width > 64 ? new Rectangle?(new Rectangle(0, 0, tile.Width / 4, tile.Height)) : null;

                spriteBatch.Draw(tile, pos, src, Color.FromNonPremultiplied(255, 255, 255, alpha));
            }
        }

        private IMapFile CurrentMap => _currentMapProvider.CurrentMap;
    }
}
