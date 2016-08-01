// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.GameExecution;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.MapEntityRenderers;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Map
{
    public class MapRenderer : DrawableGameComponent, IMapRenderer
    {
        private static readonly List<MapRenderLayer> _possibleLayers;

        static MapRenderer()
        {
            _possibleLayers = Enum.GetValues(typeof(MapRenderLayer))
                                  .OfType<MapRenderLayer>()
                                  .ToList();
        }

        private readonly IRenderTargetFactory _renderTargetFactory;
        private readonly IMapEntityRendererProvider _mapEntityRendererProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly IMapRenderDistanceCalculator _mapRenderDistanceCalculator;

        private RenderTarget2D _mapAbovePlayer, _mapBelowPlayer;
        private SpriteBatch _sb;

        public MapRenderer(IEndlessGame endlessGame,
                           IRenderTargetFactory renderTargetFactory,
                           IMapEntityRendererProvider mapEntityRendererProvider,
                           ICharacterProvider characterProvider,
                           ICurrentMapProvider currentMapProvider,
                           IMapRenderDistanceCalculator mapRenderDistanceCalculator)
            : base((Game)endlessGame)
        {
            _renderTargetFactory = renderTargetFactory;
            _mapEntityRendererProvider = mapEntityRendererProvider;
            _characterProvider = characterProvider;
            _currentMapProvider = currentMapProvider;
            _mapRenderDistanceCalculator = mapRenderDistanceCalculator;
        }

        public override void Initialize()
        {
            //todo: re-enable once all renderers are implemented!
            //if (_mapEntityRendererProvider.MapEntityRenderers.Count != _possibleLayers.Count)
            //    throw new InvalidOperationException("A map entity renderer implementation is missing!");

            _mapAbovePlayer = _renderTargetFactory.CreateRenderTarget();
            _mapBelowPlayer = _renderTargetFactory.CreateRenderTarget();
            _sb = new SpriteBatch(Game.GraphicsDevice);

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            if (Visible)
                DrawMapToRenderTarget();

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            DrawToSpriteBatch(_sb);

            base.Draw(gameTime);
        }

        public void DrawToSpriteBatch(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(_mapAbovePlayer, Vector2.Zero, Color.White);
            spriteBatch.Draw(_mapBelowPlayer, Vector2.Zero, Color.White);

            spriteBatch.End();
        }

        private void DrawMapToRenderTarget()
        {
            var immutableCharacter = _characterProvider.ActiveCharacter;

            GraphicsDevice.SetRenderTarget(_mapAbovePlayer);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            var renderBounds = _mapRenderDistanceCalculator.CalculateRenderBounds(immutableCharacter, _currentMapProvider.CurrentMap);
            for (int row = renderBounds.FirstRow; row <= renderBounds.LastRow; row++)
            {
                var localRow = row;

                _sb.Begin();

                for (int col = renderBounds.FirstCol; col <= renderBounds.LastCol; col++)
                {
                    var localCol = col;

                    if (CharacterIsAtPosition(immutableCharacter.RenderProperties, row, col))
                        SwitchRenderTargets();

                    var renderers = _mapEntityRendererProvider.MapEntityRenderers
                                                              .Where(x => x.ElementTypeIsInRange(localRow, localCol));

                    foreach (var renderer in renderers)
                        renderer.RenderElementAt(_sb, row, col, 255); //todo: alpha for fading
                }

                _sb.End();
            }

            GraphicsDevice.SetRenderTarget(null);
        }

        private static bool CharacterIsAtPosition(ICharacterRenderProperties renderProperties, int row, int col)
        {
            if (renderProperties.CurrentAction == CharacterActionState.Walking)
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mapAbovePlayer.Dispose();
                _mapBelowPlayer.Dispose();
                _sb.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}