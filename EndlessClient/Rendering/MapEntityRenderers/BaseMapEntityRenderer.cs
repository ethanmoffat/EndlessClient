// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Rendering.Map;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public abstract class BaseMapEntityRenderer : IMapEntityRenderer
    {
        protected readonly ICharacterProvider _characterProvider;
        private readonly IRenderOffsetCalculator _renderOffsetCalculator;

        public abstract MapRenderLayer RenderLayer { get; }

        public bool ShouldRenderLast => RenderLayer == MapRenderLayer.Roof || RenderLayer == MapRenderLayer.MainCharacter;

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

        public abstract void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha);

        protected Vector2 GetDrawCoordinatesFromGridUnits(int gridX, int gridY)
        {
            var charOffX = _renderOffsetCalculator.CalculateOffsetX(_characterProvider.MainCharacter.RenderProperties);
            var charOffY = _renderOffsetCalculator.CalculateOffsetY(_characterProvider.MainCharacter.RenderProperties);

            return new Vector2(gridX * 32 - gridY * 32 + 288 - charOffX,
                               gridY * 16 + gridX * 16 + 144 - charOffY);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~BaseMapEntityRenderer()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing) { }
    }
}
