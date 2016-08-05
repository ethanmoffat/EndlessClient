// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.CharacterProperties;
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

        public override MapRenderLayer RenderLayer
        {
            get { return MapRenderLayer.Ground; }
        }

        protected override int RenderDistance { get { return 10; } }

        public GroundLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                   ICurrentMapProvider currentMapProvider,
                                   ICharacterProvider characterProvider,
                                   ICharacterRenderOffsetCalculator characterRenderOffsetCalculator)
            : base(characterProvider, characterRenderOffsetCalculator)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _currentMapProvider = currentMapProvider;
        }

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
                spriteBatch.Draw(fillTile, new Vector2(pos.X - 1, pos.Y - 2), Color.FromNonPremultiplied(255, 255, 255, alpha));
            }
            else if ((tileGFX = CurrentMap.GFX[MapLayer.GroundTile][row, col]) > 0)
            {
                var tile = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapTiles, tileGFX, true);

                //todo: animate ground tiles by adjusting the source rectangle offset
                var src = tile.Width > 64 ? new Rectangle?(new Rectangle(0, 0, tile.Width / 4, tile.Height)) : null;

                spriteBatch.Draw(tile, new Vector2(pos.X - 1, pos.Y - 2), src, Color.FromNonPremultiplied(255, 255, 255, alpha));
            }
        }

        private IReadOnlyMapFile CurrentMap { get { return _currentMapProvider.CurrentMap; } }
    }
}
