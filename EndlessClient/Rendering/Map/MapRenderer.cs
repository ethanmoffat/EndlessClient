using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.MapEntityRenderers;
using EndlessClient.Rendering.NPC;
using EOLib;
using EOLib.Config;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.Rendering.Map
{
    public class MapRenderer : DrawableGameComponent, IMapRenderer
    {
        private const double TRANSITION_TIME_MS = 125.0;

        private readonly IRenderTargetFactory _renderTargetFactory;
        private readonly IMapEntityRendererProvider _mapEntityRendererProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IMapRenderDistanceCalculator _mapRenderDistanceCalculator;
        private readonly ICharacterRendererUpdater _characterRendererUpdater;
        private readonly INPCRendererUpdater _npcRendererUpdater;
        private readonly IDoorStateUpdater _doorStateUpdater;
        private readonly IChatBubbleUpdater _chatBubbleUpdater;
        private readonly IConfigurationProvider _configurationProvider;
        private readonly IMouseCursorRenderer _mouseCursorRenderer;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;

        private RenderTarget2D _mapBaseTarget, _mapObjectTarget;
        private SpriteBatch _sb;
        private MapTransitionState _mapTransitionState = MapTransitionState.Default;
        private int? _lastMapChecksum;

        private bool MouseOver
        {
            get
            {
                var ms = Mouse.GetState();
                //todo: turn magic numbers into meaningful values
                return Game.IsActive && ms.X > 0 && ms.Y > 0 && ms.X < 640 && ms.Y < 320;
            }
        }

        public MapRenderer(IEndlessGame endlessGame,
                           IRenderTargetFactory renderTargetFactory,
                           IMapEntityRendererProvider mapEntityRendererProvider,
                           ICharacterProvider characterProvider,
                           ICurrentMapProvider currentMapProvider,
                           IMapRenderDistanceCalculator mapRenderDistanceCalculator,
                           ICharacterRendererUpdater characterRendererUpdater,
                           INPCRendererUpdater npcRendererUpdater,
                           IDoorStateUpdater doorStateUpdater,
                           IChatBubbleUpdater chatBubbleUpdater,
                           IConfigurationProvider configurationProvider,
                           IMouseCursorRenderer mouseCursorRenderer,
                           IRenderOffsetCalculator renderOffsetCalculator)
            : base((Game)endlessGame)
        {
            _renderTargetFactory = renderTargetFactory;
            _mapEntityRendererProvider = mapEntityRendererProvider;
            _characterProvider = characterProvider;
            _currentMapProvider = currentMapProvider;
            _mapRenderDistanceCalculator = mapRenderDistanceCalculator;
            _characterRendererUpdater = characterRendererUpdater;
            _npcRendererUpdater = npcRendererUpdater;
            _doorStateUpdater = doorStateUpdater;
            _chatBubbleUpdater = chatBubbleUpdater;
            _configurationProvider = configurationProvider;
            _mouseCursorRenderer = mouseCursorRenderer;
            _renderOffsetCalculator = renderOffsetCalculator;
        }

        public override void Initialize()
        {
            _mapBaseTarget = _renderTargetFactory.CreateRenderTarget();
            _mapObjectTarget = _renderTargetFactory.CreateRenderTarget();
            _sb = new SpriteBatch(Game.GraphicsDevice);

            _mouseCursorRenderer.Initialize();

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (!_lastMapChecksum.HasValue || _lastMapChecksum != _currentMapProvider.CurrentMap.Properties.ChecksumInt)
            {
                // The dimensions of the map are 0-based in the properties. Adjust to 1-based for RT creation
                var widthPlus1 = _currentMapProvider.CurrentMap.Properties.Width + 1;
                var heightPlus1 = _currentMapProvider.CurrentMap.Properties.Height + 1;

                _mapBaseTarget.Dispose();
                _mapBaseTarget = _renderTargetFactory.CreateRenderTarget(
                    (widthPlus1 + heightPlus1) * 32,
                    (widthPlus1 + heightPlus1) * 16);
            }

            if (Visible)
            {
                _characterRendererUpdater.UpdateCharacters(gameTime);
                _npcRendererUpdater.UpdateNPCs(gameTime);
                _doorStateUpdater.UpdateDoorState(gameTime);
                _chatBubbleUpdater.UpdateChatBubbles(gameTime);

                if (MouseOver)
                    _mouseCursorRenderer.Update(gameTime);
            }

            _lastMapChecksum = _currentMapProvider.CurrentMap.Properties.ChecksumInt;

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            DrawGroundLayerToRenderTarget();
            DrawMapToRenderTarget();
            DrawToSpriteBatch(_sb, gameTime);

            base.Draw(gameTime);
        }

        public void StartMapTransition()
        {
            _mapTransitionState = new MapTransitionState(DateTime.Now, 1);
        }

        private void DrawToSpriteBatch(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(_mapBaseTarget, GetGroundLayerDrawPosition(), Color.White);
            DrawBaseLayers(spriteBatch);

            _mouseCursorRenderer.Draw(spriteBatch, gameTime);

            spriteBatch.Draw(_mapObjectTarget, Vector2.Zero, Color.White);

            spriteBatch.End();
        }

        private void DrawGroundLayerToRenderTarget()
        {
            if (!_mapTransitionState.StartTime.HasValue && _lastMapChecksum == _currentMapProvider.CurrentMap.Properties.ChecksumInt)
                return;

            GraphicsDevice.SetRenderTarget(_mapBaseTarget);
            _sb.Begin();

            var renderBounds = new MapRenderBounds(0, _currentMapProvider.CurrentMap.Properties.Height,
                                                   0, _currentMapProvider.CurrentMap.Properties.Width);

            var transitionComplete = true;
            for (var row = renderBounds.FirstRow; row <= renderBounds.LastRow; row++)
            {
                for (var col = renderBounds.FirstCol; col <= renderBounds.LastCol; ++col)
                {
                    var alpha = GetAlphaForCoordinates(col, row, _characterProvider.MainCharacter);
                    transitionComplete &= alpha == 255;

                    if (_mapEntityRendererProvider.GroundRenderer.CanRender(row, col))
                        _mapEntityRendererProvider.GroundRenderer.RenderElementAt(_sb, row, col, alpha);
                }
            }

            if (transitionComplete)
                _mapTransitionState = new MapTransitionState(Optional<DateTime>.Empty, 0);

            _sb.End();
            GraphicsDevice.SetRenderTarget(null);
        }

        private void DrawBaseLayers(SpriteBatch spriteBatch)
        {
            var renderBounds = _mapRenderDistanceCalculator.CalculateRenderBounds(_characterProvider.MainCharacter, _currentMapProvider.CurrentMap);

            for (var row = renderBounds.FirstRow; row <= renderBounds.LastRow; row++)
            {
                for (var col = renderBounds.FirstCol; col <= renderBounds.LastCol; ++col)
                {
                    var alpha = GetAlphaForCoordinates(col, row, _characterProvider.MainCharacter);

                    foreach (var renderer in _mapEntityRendererProvider.BaseRenderers)
                    {
                        if (renderer.CanRender(row, col))
                            renderer.RenderElementAt(spriteBatch, row, col, alpha);
                    }
                }
            }
        }

        private void DrawMapToRenderTarget()
        {
            var immutableCharacter = _characterProvider.MainCharacter;

            GraphicsDevice.SetRenderTarget(_mapObjectTarget);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            var gfxToRenderLast = new SortedList<Point, List<MapRenderLayer>>(new PointComparer());

            _sb.Begin();

            var renderBounds = _mapRenderDistanceCalculator.CalculateRenderBounds(immutableCharacter, _currentMapProvider.CurrentMap);
            for (var row = renderBounds.FirstRow; row <= renderBounds.LastRow; row++)
            {
                for (var col = renderBounds.FirstCol; col <= renderBounds.LastCol; col++)
                {
                    var alpha = GetAlphaForCoordinates(col, row, immutableCharacter);

                    foreach (var renderer in _mapEntityRendererProvider.MapEntityRenderers)
                    {
                        if (!renderer.CanRender(row, col))
                            continue;

                        if (renderer.ShouldRenderLast)
                        {
                            var renderLaterKey = new Point(col, row);
                            if (gfxToRenderLast.ContainsKey(renderLaterKey))
                                gfxToRenderLast[renderLaterKey].Add(renderer.RenderLayer);
                            else
                                gfxToRenderLast.Add(renderLaterKey, new List<MapRenderLayer> { renderer.RenderLayer });
                        }
                        else
                            renderer.RenderElementAt(_sb, row, col, alpha);
                    }
                }
            }

            foreach (var kvp in gfxToRenderLast)
            {
                var pointKey = kvp.Key;
                var alpha = GetAlphaForCoordinates(pointKey.X, pointKey.Y, immutableCharacter);

                foreach (var layer in kvp.Value)
                {
                    _mapEntityRendererProvider.MapEntityRenderers
                                              .Single(x => x.RenderLayer == layer)
                                              .RenderElementAt(_sb, pointKey.Y, pointKey.X, alpha);
                }
            }

            _sb.End();
            GraphicsDevice.SetRenderTarget(null);
        }

        private static bool CharacterIsAtPosition(ICharacterRenderProperties renderProperties, int row, int col)
        {
            if (renderProperties.IsActing(CharacterActionState.Walking))
                return row == renderProperties.GetDestinationY() && col == renderProperties.GetDestinationX();

            return row == renderProperties.MapY && col == renderProperties.MapX;
        }

        private Vector2 GetGroundLayerDrawPosition()
        {
            // TODO: update for dynamic viewport sizing
            const int ViewportWidthFactor = 320; // 640 * (1/2)
            const int ViewportHeightFactor = 144; // 480 * (3/10)

            var props = _characterProvider.MainCharacter.RenderProperties;
            var charOffX = _renderOffsetCalculator.CalculateWalkAdjustX(props);
            var charOffY = _renderOffsetCalculator.CalculateWalkAdjustY(props);

            var mapHeightPlusOne = _currentMapProvider.CurrentMap.Properties.Height + 1;

            // X coordinate: +32 per Y, -32 per X
            // Y coordinate: -16 per Y, -16 per X
            // basically the opposite of the algorithm for rendering the ground tiles
            return new Vector2(ViewportWidthFactor - (mapHeightPlusOne * 32) + (props.MapY * 32) - (props.MapX * 32) - charOffX,
                               ViewportHeightFactor - (props.MapY * 16) - (props.MapX * 16) - charOffY);
        }

        private int GetAlphaForCoordinates(int objX, int objY, ICharacter character)
        {
            if (!_configurationProvider.ShowTransition)
            {
                _mapTransitionState = new MapTransitionState(Optional<DateTime>.Empty, 0);
                return 255;
            }

            //get the farther away of X or Y coordinate for the map object
            var metric = Math.Max(Math.Abs(objX - character.RenderProperties.MapX),
                                  Math.Abs(objY - character.RenderProperties.MapY));

            int alpha;
            if (!_mapTransitionState.StartTime.HasValue ||
                metric < _mapTransitionState.TransitionMetric ||
                _mapTransitionState.TransitionMetric == 0)
            {
                alpha = 255;
            }
            else if (metric == _mapTransitionState.TransitionMetric)
            {
                var ms = (DateTime.Now - _mapTransitionState.StartTime).TotalMilliseconds;
                alpha = (int)Math.Round(ms / TRANSITION_TIME_MS * 255);

                if (ms / TRANSITION_TIME_MS >= 1)
                    _mapTransitionState = new MapTransitionState(DateTime.Now, _mapTransitionState.TransitionMetric + 1);
            }
            else
                alpha = 0;

            return alpha;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mapBaseTarget.Dispose();
                _mapObjectTarget.Dispose();
                _sb.Dispose();
                _mouseCursorRenderer.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    internal struct MapTransitionState
    {
        public static MapTransitionState Default => new MapTransitionState(Optional<DateTime>.Empty, 0);

        public Optional<DateTime> StartTime { get; }

        public int TransitionMetric { get; }

        public MapTransitionState(Optional<DateTime> startTime, int transitionMetric)
            : this()
        {
            StartTime = startTime;
            TransitionMetric = transitionMetric;
        }
    }
}