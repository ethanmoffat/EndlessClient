// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
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
    public class GroundLayerRenderer : IMapEntityRenderer
    {
        private const int RENDER_DISTANCE = 10;

        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IMapFileProvider _mapFileProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICharacterRenderOffsetCalculator _characterRenderOffsetCalculator;

        public MapRenderLayer RenderLayer
        {
            get { return MapRenderLayer.Ground; }
        }

        public GroundLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                   IMapFileProvider mapFileProvider,
                                   ICharacterProvider characterProvider,
                                   ICharacterRenderOffsetCalculator characterRenderOffsetCalculator)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _mapFileProvider = mapFileProvider;
            _characterProvider = characterProvider;
            _characterRenderOffsetCalculator = characterRenderOffsetCalculator;
        }

        //todo: put this in a base class and make RENDER_DISTANCE protected abstract or part of the interface?
        public bool ElementTypeIsInRange(int row, int col)
        {
            var props = _characterProvider.ActiveCharacter.RenderProperties;

            var rowDelta = Math.Abs(props.MapY - row);
            var colDelta = Math.Abs(props.MapX - col);

            return rowDelta <= RENDER_DISTANCE && colDelta <= RENDER_DISTANCE;
        }

        public void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
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

        //todo: put this in a base class?
        private static Vector2 GetDrawCoordinatesFromGridUnits(int gridX, int gridY, int characterOffsetX, int characterOffsetY)
        {
            return new Vector2(gridX*32 - gridY*32 + 288 - characterOffsetX,
                               gridY*16 + gridX*16 + 144 - characterOffsetY);
        }

        //todo: put this in a base class?
        private IReadOnlyMapFile MapFile { get { return _mapFileProvider.MapFiles[_characterProvider.ActiveCharacter.MapID]; } }
    }
}
