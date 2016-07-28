// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Map
{
    public interface IMapRenderer : IDrawable, IUpdateable, IGameComponent, IDisposable
    {
        void DrawToSpriteBatch(SpriteBatch spriteBatch);
    }
}
