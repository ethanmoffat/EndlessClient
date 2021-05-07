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
    public abstract class WallLayerRendererBase : BaseMapEntityRenderer
    {
        private const int WALL_FRAME_WIDTH = 68;

        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;

        protected override int RenderDistance => 20;

        protected WallLayerRendererBase(INativeGraphicsManager nativeGraphicsManager,
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

        protected void DrawWall(SpriteBatch spriteBatch, int row, int col, int alpha, int gfxNum, Vector2 additionalOffset = default)
        {
            if (_currentMapStateProvider.OpenDoors.Any(openDoor => openDoor.X == col && openDoor.Y == row))
                gfxNum++;

            var gfx = _nativeGraphicsManager.TextureFromResource(GFXTypes.MapWalls, gfxNum, true);

            var gfxWidthDelta = gfx.Width/4;
            var src = gfx.Width > WALL_FRAME_WIDTH
                ? new Rectangle?(new Rectangle(gfxWidthDelta * _frameIndex, 0, gfxWidthDelta, gfx.Height))
                : null;

            var pos = GetDrawCoordinatesFromGridUnits(col, row);
            pos -= new Vector2(32, gfx.Height - 32);

            spriteBatch.Draw(gfx, pos + additionalOffset, src, Color.FromNonPremultiplied(255, 255, 255, alpha));
        }

        protected IMapFile CurrentMap => _currentMapProvider.CurrentMap;
    }

    public class DownWallLayerRenderer : WallLayerRendererBase
    {
        public override MapRenderLayer RenderLayer => MapRenderLayer.DownWall;

        public DownWallLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                    ICurrentMapProvider currentMapProvider,
                                    ICharacterProvider characterProvider,
                                    IRenderOffsetCalculator renderOffsetCalculator,
                                    IClientWindowSizeProvider clientWindowSizeProvider,
                                    ICurrentMapStateProvider currentMapStateProvider)
            : base(nativeGraphicsManager, currentMapProvider, characterProvider, renderOffsetCalculator, clientWindowSizeProvider, currentMapStateProvider)
        {
        }

        protected override bool ElementExistsAt(int row, int col)
        {
            return CurrentMap.GFX[MapLayer.WallRowsDown][row, col] > 0;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            base.RenderElementAt(spriteBatch, row, col, alpha, additionalOffset);

            var gfxNum = CurrentMap.GFX[MapLayer.WallRowsDown][row, col];
            DrawWall(spriteBatch, row, col, alpha, gfxNum, additionalOffset);
        }
    }

    public class RightWallLayerRenderer : WallLayerRendererBase
    {
        public override MapRenderLayer RenderLayer => MapRenderLayer.RightWall;

        public RightWallLayerRenderer(INativeGraphicsManager nativeGraphicsManager,
                                      ICurrentMapProvider currentMapProvider,
                                      ICharacterProvider characterProvider,
                                      IRenderOffsetCalculator renderOffsetCalculator,
                                      IClientWindowSizeProvider clientWindowSizeProvider,
                                      ICurrentMapStateProvider currentMapStateProvider)
            : base(nativeGraphicsManager, currentMapProvider, characterProvider, renderOffsetCalculator, clientWindowSizeProvider, currentMapStateProvider)
        {
        }

        protected override bool ElementExistsAt(int row, int col)
        {
            return CurrentMap.GFX[MapLayer.WallRowsRight][row, col] > 0;
        }

        public override void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            base.RenderElementAt(spriteBatch, row, col, alpha, additionalOffset);

            var gfxNum = CurrentMap.GFX[MapLayer.WallRowsRight][row, col];
            DrawWall(spriteBatch, row, col, alpha, gfxNum, additionalOffset);
        }
    }
}
