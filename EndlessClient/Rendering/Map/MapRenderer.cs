// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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

        private RenderTarget2D _mapAbovePlayer, _mapBelowPlayer;
        private SpriteBatch _sb;
        private MapTransitionState _mapTransitionState = MapTransitionState.Default;

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
                           IMouseCursorRenderer mouseCursorRenderer)
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
        }

        public override void Initialize()
        {
            _mapAbovePlayer = _renderTargetFactory.CreateRenderTarget();
            _mapBelowPlayer = _renderTargetFactory.CreateRenderTarget();
            _sb = new SpriteBatch(Game.GraphicsDevice);

            _mouseCursorRenderer.Initialize();

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (Visible)
            {
                _characterRendererUpdater.UpdateCharacters(gameTime);
                _npcRendererUpdater.UpdateNPCs(gameTime);
                _doorStateUpdater.UpdateDoorState(gameTime);
                _chatBubbleUpdater.UpdateChatBubbles(gameTime);

                if (MouseOver)
                    _mouseCursorRenderer.Update(gameTime);

                DrawMapToRenderTarget();
            }

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            DrawMapBase(_sb);
            _mouseCursorRenderer.Draw(gameTime);
            DrawToSpriteBatch(_sb);

            base.Draw(gameTime);
        }

        public void StartMapTransition()
        {
            _mapTransitionState = new MapTransitionState(DateTime.Now, 1);
        }

        private void DrawToSpriteBatch(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(_mapAbovePlayer, Vector2.Zero, Color.White);
            spriteBatch.Draw(_mapBelowPlayer, Vector2.Zero, Color.White);

            spriteBatch.End();
        }

        private void DrawMapBase(SpriteBatch spriteBatch)
        {
            var immutableCharacter = _characterProvider.MainCharacter;
            var renderBounds = _mapRenderDistanceCalculator.CalculateRenderBounds(immutableCharacter, _currentMapProvider.CurrentMap);

            for (var row = renderBounds.FirstRow; row <= renderBounds.LastRow; row++)
            {
                spriteBatch.Begin();

                for (var col = renderBounds.FirstCol; col <= renderBounds.LastCol; ++col)
                {
                    var alpha = GetAlphaForCoordinates(col, row, immutableCharacter);

                    foreach (var renderer in _mapEntityRendererProvider.MapBaseRenderers)
                    {
                        if (!renderer.CanRender(row, col))
                            continue;

                        renderer.RenderElementAt(spriteBatch, row, col, alpha);
                    }
                }

                spriteBatch.End();
            }
        }

        private void DrawMapToRenderTarget()
        {
            var immutableCharacter = _characterProvider.MainCharacter;

            GraphicsDevice.SetRenderTarget(_mapAbovePlayer);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            var gfxToRenderLast = new SortedList<Point, List<MapRenderLayer>>(new PointComparer());

            var renderBounds = _mapRenderDistanceCalculator.CalculateRenderBounds(immutableCharacter, _currentMapProvider.CurrentMap);
            for (var row = renderBounds.FirstRow; row <= renderBounds.LastRow; row++)
            {
                _sb.Begin();

                for (var col = renderBounds.FirstCol; col <= renderBounds.LastCol; col++)
                {
                    if (CharacterIsAtPosition(immutableCharacter.RenderProperties, row, col))
                        SwitchRenderTargets();

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

                _sb.End();
            }

            _sb.Begin();
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

        private void SwitchRenderTargets()
        {
            _sb.End();

            GraphicsDevice.SetRenderTarget(_mapBelowPlayer);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            _sb.Begin();
        }

        private int GetAlphaForCoordinates(int objX, int objY, ICharacter character)
        {
            if (!_configurationProvider.ShowTransition)
                return 255;

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
                alpha = (int) Math.Round(ms/TRANSITION_TIME_MS*255);
                
                if (ms/TRANSITION_TIME_MS >= 1)
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
                _mapAbovePlayer.Dispose();
                _mapBelowPlayer.Dispose();
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