// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Chat
{
    public interface IChatRenderable
    {
        int DisplayIndex { get; }

        void Render(SpriteBatch spriteBatch, SpriteFont chatFont, INativeGraphicsManager nativeGraphicsManager);

        void SetDisplayIndex(int newIndex);
    }
}