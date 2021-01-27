using System;
using System.Collections.Generic;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public abstract class BaseMapEntityRenderer : IMapEntityRenderer
    {
        private static readonly Dictionary<MapRenderLayer, Point> _layerOffsets;

        private static DateTime _lastFrameTime = DateTime.Now;
        protected static int _frameIndex = 0;

        static BaseMapEntityRenderer()
        {
            _layerOffsets = new Dictionary<MapRenderLayer, Point>
            {
                { MapRenderLayer.Ground, Point.Zero },
                { MapRenderLayer.Item, new Point(0, 16) },
                { MapRenderLayer.Objects, new Point(-2, -2) },
                { MapRenderLayer.Overlay, new Point(-2, -2) },
                { MapRenderLayer.DownWall, new Point(0, -1) },
                { MapRenderLayer.RightWall, new Point(32, -1) },
                { MapRenderLayer.Roof, new Point(-32, -64) },
                { MapRenderLayer.OnTop, new Point(-32, -32) },
                { MapRenderLayer.Shadows, new Point(-24, -12) },
                { MapRenderLayer.Overlay2, new Point(0, -64) },
                { MapRenderLayer.MainCharacter, Point.Zero },
            };
        }

        protected readonly ICharacterProvider _characterProvider;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;

        public abstract MapRenderLayer RenderLayer { get; }

        public bool ShouldRenderLast => RenderLayer == MapRenderLayer.Overlay2 || RenderLayer == MapRenderLayer.MainCharacterTransparent;

        protected abstract int RenderDistance { get; }

        protected BaseMapEntityRenderer(ICharacterProvider characterProvider,
                                        IRenderOffsetCalculator renderOffsetCalculator)
        {
            _characterProvider = characterProvider;
            _renderOffsetCalculator = renderOffsetCalculator;
        }

        public virtual bool CanRender(int row, int col)
        {
            if (!ElementExistsAt(row, col))
                return false;

            var props = _characterProvider.MainCharacter.RenderProperties;

            var rowDelta = Math.Abs(props.MapY - row);
            var colDelta = Math.Abs(props.MapX - col);

            return rowDelta <= RenderDistance && colDelta <= RenderDistance;
        }

        protected abstract bool ElementExistsAt(int row, int col);

        public virtual void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalOffset = default)
        {
            if ((DateTime.Now - _lastFrameTime).TotalMilliseconds > 500)
            {
                _lastFrameTime = DateTime.Now;
                _frameIndex = (_frameIndex + 1) % 4;
            }
        }

        protected virtual Vector2 GetDrawCoordinatesFromGridUnits(int gridX, int gridY)
        {
            const int ViewportWidthFactor = 320; // 640 * (1/2)
            const int ViewportHeightFactor = 144; // 480 * (3/10)

            var charOffX = _renderOffsetCalculator.CalculateOffsetX(_characterProvider.MainCharacter.RenderProperties);
            var charOffY = _renderOffsetCalculator.CalculateOffsetY(_characterProvider.MainCharacter.RenderProperties);

            return new Vector2(ViewportWidthFactor + (gridX * 32) - (gridY * 32) - charOffX + _layerOffsets[RenderLayer].X,
                               ViewportHeightFactor + (gridY * 16) + (gridX * 16) - charOffY + _layerOffsets[RenderLayer].Y);
        }
    }
}
