using System;
using EndlessClient.Rendering.Map;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.MapEntityRenderers
{
    public interface IMapEntityRenderer : IDisposable
    {
        MapRenderLayer RenderLayer { get; }

        bool ShouldRenderLast { get; }

        bool CanRender(int row, int col);

        void RenderElementAt(SpriteBatch spriteBatch, int row, int col, int alpha);
    }
}
