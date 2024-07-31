using EndlessClient.Rendering.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public interface IMapEntityRenderer
    {
        MapRenderLayer RenderLayer { get; }

        bool ShouldRenderLast { get; }

        bool CanRender(int row, int col);

        void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha, Vector2 additionalPixelOffset = default);
    }
}