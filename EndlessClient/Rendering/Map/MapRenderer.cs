// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Rendering.Factories;
using EndlessClient.Rendering.MapEntityRenderers;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.Rendering.Map
{
    //todo: maybe this should just be a DGC instead of an XNAControl, like it used to be...
    public class MapRenderer : XNAControl, IMapRenderer
    {
        private static readonly List<MapRenderLayer> _possibleLayers;

        static MapRenderer()
        {
            _possibleLayers = Enum.GetValues(typeof(MapRenderLayer))
                                  .OfType<MapRenderLayer>()
                                  .ToList();
        }

        private readonly IMapRenderTargetFactory _mapRenderTargetFactory;
        private readonly IMapEntityRendererProvider _mapEntityRendererProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IMapFileProvider _mapFileProvider;
        private readonly ICurrentMapStateProvider _mapStateProvider;
        private readonly IMapRenderDistanceCalculator _mapRenderDistanceCalculator;

        private RenderTarget2D _mapAbovePlayer, _mapBelowPlayer;

        public MapRenderer(IMapRenderTargetFactory mapRenderTargetFactory,
                           IMapEntityRendererProvider mapEntityRendererProvider,
                           ICharacterProvider characterProvider,
                           IMapFileProvider mapFileProvider,
                           ICurrentMapStateProvider mapStateProvider,
                           IMapRenderDistanceCalculator mapRenderDistanceCalculator)
        {
            _mapRenderTargetFactory = mapRenderTargetFactory;
            _mapEntityRendererProvider = mapEntityRendererProvider;
            _characterProvider = characterProvider;
            _mapFileProvider = mapFileProvider;
            _mapStateProvider = mapStateProvider;
            _mapRenderDistanceCalculator = mapRenderDistanceCalculator;
        }

        public override void Initialize()
        {
            if (_mapEntityRendererProvider.MapEntityRenderers.Count != _possibleLayers.Count)
                throw new InvalidOperationException("A map entity renderer implementation is missing!");

            _mapAbovePlayer = _mapRenderTargetFactory.CreateMapRenderTarget();
            _mapBelowPlayer = _mapRenderTargetFactory.CreateMapRenderTarget();

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

            DrawToSpriteBatch(SpriteBatch);

            base.Draw(gameTime);
        }

        public void DrawToSpriteBatch(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin();

            spriteBatch.Draw(_mapAbovePlayer, Vector2.Zero, Color.White);
            spriteBatch.Draw(_mapBelowPlayer, Vector2.Zero, Color.White);
            
            //todo: draw FPS string

            spriteBatch.End();
        }

        private void DrawMapToRenderTarget()
        {
            var immutableCharacter = _characterProvider.ActiveCharacter;

            GraphicsDevice.SetRenderTarget(_mapAbovePlayer);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            var renderBounds = _mapRenderDistanceCalculator.CalculateRenderBounds(immutableCharacter, CurrentMap);
            for (int row = renderBounds.FirstRow; row <= renderBounds.LastRow; row++)
            {
                SpriteBatch.Begin();

                for (int col = renderBounds.FirstCol; col <= renderBounds.LastCol; col++)
                {
                    if (CharacterIsAtPosition(immutableCharacter, row, col))
                        SwitchRenderTargets();

                    foreach (var layer in _possibleLayers)
                    {
                        _mapEntityRendererProvider.MapEntityRenderers
                                                  .Single(x => x.RenderLayer == layer)
                                                  .RenderElementAt(row, col);
                    }
                }

                SpriteBatch.End();
            }

            GraphicsDevice.SetRenderTarget(null);
        }

        private static bool CharacterIsAtPosition(ICharacter character, int row, int col)
        {
            if (character.RenderProperties.CurrentAction == CharacterActionState.Walking)
                return row == character.GetDestinationY() && col == character.GetDestinationX();

            return row == character.MapY && col == character.MapX;
        }

        private void SwitchRenderTargets()
        {
            SpriteBatch.End();

            GraphicsDevice.SetRenderTarget(_mapBelowPlayer);
            GraphicsDevice.Clear(ClearOptions.Target, Color.Transparent, 0, 0);

            SpriteBatch.Begin();
        }

        private IReadOnlyMapFile CurrentMap
        {
            get { return _mapFileProvider.MapFiles[_mapStateProvider.CurrentMapID]; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _mapAbovePlayer.Dispose();
                _mapBelowPlayer.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}