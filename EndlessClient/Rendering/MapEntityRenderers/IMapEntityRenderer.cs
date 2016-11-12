// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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
