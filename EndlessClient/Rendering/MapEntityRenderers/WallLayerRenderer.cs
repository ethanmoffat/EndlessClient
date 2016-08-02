// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Map;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public class WallLayerRenderer : BaseMapEntityRenderer
    {
        private const int WALL_FRAME_WIDTH = 68;

        private readonly INativeGraphicsManager _nativeGraphicsManager;

        public override MapRenderLayer RenderLayer
        {
            get { return MapRenderLayer.Walls; }
        }

        protected override int RenderDistance
        {
            get { return 20; }
        }

        public WallLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                 IMapFileProvider mapFileProvider,
                                 ICharacterProvider characterProvider,
                                 ICharacterRenderOffsetCalculator characterRenderOffsetCalculator)
            : base(mapFileProvider, characterProvider, characterRenderOffsetCalculator)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha)
        {
            int gfxNum;
            if ((gfxNum = MapFile.GFX[MapLayer.WallRowsRight][row, col]) > 0)
                DrawWall(spriteBatch, row, col, alpha, gfxNum, MapLayer.WallRowsRight);

            if ((gfxNum = MapFile.GFX[MapLayer.WallRowsDown][row, col]) > 0)
                DrawWall(spriteBatch, row, col, alpha, gfxNum, MapLayer.WallRowsDown);
        }

        private void DrawWall(SpriteBatch spriteBatch, int row, int col, int alpha, int gfxNum, MapLayer renderLayer)
        {
            if (renderLayer != MapLayer.WallRowsDown && renderLayer != MapLayer.WallRowsRight)
                throw new ArgumentOutOfRangeException("renderLayer", "renderLayer must be WallRowsDown or WallRowsRight");

            var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapWalls, gfxNum, true);
            var pos = GetDrawCoordinatesFromGridUnits(col, row);

            var gfxWidthDelta = gfx.Width/4;
            var src = gfx.Width > WALL_FRAME_WIDTH
                ? new Rectangle?(new Rectangle(gfxWidthDelta*0, 0, gfxWidthDelta, gfx.Height)) //todo: animated walls using source index offset!
                : null;

            var wallXOffset = renderLayer == MapLayer.WallRowsRight ? 47 : 15;
            var wallAnimationAdjust = (int) Math.Round((gfx.Width > WALL_FRAME_WIDTH ? gfxWidthDelta : gfx.Width)/2.0);
            pos = new Vector2(pos.X - wallAnimationAdjust + wallXOffset,
                              pos.Y - (gfx.Height - 29));

            spriteBatch.Draw(gfx, pos, src, Color.FromNonPremultiplied(255, 255, 255, alpha));
        }
    }
}
