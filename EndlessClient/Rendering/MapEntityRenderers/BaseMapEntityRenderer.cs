// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Rendering.CharacterProperties;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using EOLib.IO.Map;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public abstract class BaseMapEntityRenderer : IMapEntityRenderer
    {
        private readonly IMapFileProvider _mapFileProvider;
        protected readonly ICharacterProvider _characterProvider;
        private readonly ICharacterRenderOffsetCalculator _characterRenderOffsetCalculator;

        public abstract MapRenderLayer RenderLayer { get; }

        protected abstract int RenderDistance { get; }

        protected BaseMapEntityRenderer(IMapFileProvider mapFileProvider,
                                        ICharacterProvider characterProvider,
                                        ICharacterRenderOffsetCalculator characterRenderOffsetCalculator)
        {
            _mapFileProvider = mapFileProvider;
            _characterProvider = characterProvider;
            _characterRenderOffsetCalculator = characterRenderOffsetCalculator;
        }

        public bool ElementTypeIsInRange(int row, int col)
        {
            var props = _characterProvider.ActiveCharacter.RenderProperties;

            var rowDelta = Math.Abs(props.MapY - row);
            var colDelta = Math.Abs(props.MapX - col);

            return rowDelta <= RenderDistance && colDelta <= RenderDistance;
        }

        public abstract void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha);

        protected Vector2 GetDrawCoordinatesFromGridUnits(int gridX, int gridY)
        {
            var charOffX = _characterRenderOffsetCalculator.CalculateOffsetX(_characterProvider.ActiveCharacter.RenderProperties);
            var charOffY = _characterRenderOffsetCalculator.CalculateOffsetY(_characterProvider.ActiveCharacter.RenderProperties);

            return new Vector2(gridX * 32 - gridY * 32 + 288 - charOffX,
                               gridY * 16 + gridX * 16 + 144 - charOffY);
        }

        protected IReadOnlyMapFile MapFile { get { return _mapFileProvider.MapFiles[_characterProvider.ActiveCharacter.MapID]; } }
    }
}
