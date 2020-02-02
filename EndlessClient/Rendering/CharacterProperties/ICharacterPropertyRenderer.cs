// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public interface ICharacterPropertyRenderer
    {
        bool CanRender { get; }

        float LayerDepth { get; set; }

        void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea);
    }
}
