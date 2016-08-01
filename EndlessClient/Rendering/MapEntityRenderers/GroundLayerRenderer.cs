// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class GroundLayerRenderer : BaseMapEntityRenderer
    {
        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICharacterRenderOffsetCalculator _characterRenderOffsetCalculator;

        public override MapRenderLayer RenderLayer
        {
            get { return MapRenderLayer.Ground; }
        }

        public override int RenderDistance { get { return 10; } }

        public GroundLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                   IMapFileProvider mapFileProvider,
                                   ICharacterProvider characterProvider,
                                   ICharacterRenderOffsetCalculator characterRenderOffsetCalculator)
            : base(mapFileProvider, characterProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _characterRenderOffsetCalculator = characterRenderOffsetCalculator;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
        {
            var offX = _characterRenderOffsetCalculator.CalculateOffsetX(_characterProvider.ActiveCharacter.RenderProperties);
            var offY = _characterRenderOffsetCalculator.CalculateOffsetY(_characterProvider.ActiveCharacter.RenderProperties);
            var pos = GetDrawCoordinatesFromGridUnits(col, row, offX, offY);

            int tileGFX;
            if ((tileGFX = MapFile.Properties.FillTile) > 0 && MapFile.GFX[MapLayer.GroundTile][row, col] < 0)
            {
                //todo: source rectangle for fill tile
                var fillTile = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapTiles, tileGFX, true);
                spriteBatch.Draw(fillTile, new Vector2(pos.X - 1, pos.Y - 2), Color.FromNonPremultiplied(255, 255, 255, alpha));
            }
            else if ((tileGFX = MapFile.GFX[MapLayer.GroundTile][row, col]) > 0)
            {
                var tile = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapTiles, tileGFX, true);

                //todo: animate ground tiles by adjusting the source rectangle offset
                var src = tile.Width > 64 ? new Rectangle?(new Rectangle(0, 0, tile.Width / 4, tile.Height)) : null;

                spriteBatch.Draw(tile, new Vector2(pos.X - 1, pos.Y - 2), src, Color.FromNonPremultiplied(255, 255, 255, alpha));
            }
        }
    }
}
