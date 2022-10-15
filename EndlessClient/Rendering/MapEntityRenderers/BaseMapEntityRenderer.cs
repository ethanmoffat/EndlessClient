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
                { MapRenderLayer.Overlay2, new Point(-2, -2) },
                { MapRenderLayer.MainCharacter, Point.Zero },
            };
        }

        protected readonly ICharacterProvider _characterProvider;
        protected readonly IGridDrawCoordinateCalculator _gridDrawCoordinateCalculator;
        private readonly IClientWindowSizeProvider _clientWindowSizeProvider;

        public abstract MapRenderLayer RenderLayer { get; }

        public bool ShouldRenderLast => RenderLayer == MapRenderLayer.Overlay2 || RenderLayer == MapRenderLayer.MainCharacterTransparent;

        protected abstract int RenderDistance { get; }

        protected BaseMapEntityRenderer(ICharacterProvider characterProvider,
                                        IGridDrawCoordinateCalculator gridDrawCoordinateCalculator,
                                        IClientWindowSizeProvider clientWindowSizeProvider)
        {
            _characterProvider = characterProvider;
            _gridDrawCoordinateCalculator = gridDrawCoordinateCalculator;
            _clientWindowSizeProvider = clientWindowSizeProvider;
        }

        public virtual bool CanRender(int row, int col)
        {
            if (!ElementExistsAt(row, col))
                return false;

            var props = _characterProvider.MainCharacter.RenderProperties;

            var rowDelta = Math.Abs(props.MapY - row);
            var colDelta = Math.Abs(props.MapX - col);

            // todo list:
            // 0. Uninject ClientWindowSizeProvider from places its not needed. MapRenderer, EntityRenderers
            // 10. configurable option for resizable client or not (and ensure old fixed size works still)
            // 11. clicking through hud panels to map (drag+drop inventory)

            // done list:
            // 1. fix the scaled render distance thing here
            // 2. mouse cursor renderer
            // 3. minimap renderer
            // 4. npc rendering coordinates
            // 5. minimum window size
            // 6. right side hud buttons not adjusting on bigger size
            // 7. persist hud panels instead of just one at a time
            // 8. character renderer slight offset
            // 9. other character renderers
            // 11. animated ground tiles

            var renderDistanceScaledX = (int)Math.Ceiling(_clientWindowSizeProvider.Width / 640.0 * RenderDistance);
            var renderDistanceScaledY = (int)Math.Ceiling(_clientWindowSizeProvider.Height / 480.0 * RenderDistance);

            return rowDelta <= renderDistanceScaledX && colDelta <= renderDistanceScaledY;
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
            return _gridDrawCoordinateCalculator.CalculateDrawCoordinatesFromGridUnits(gridX, gridY) + _layerOffsets[RenderLayer].ToVector2();
        }
    }
}
