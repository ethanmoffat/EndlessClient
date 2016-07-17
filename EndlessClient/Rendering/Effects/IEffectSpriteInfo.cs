// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
    public interface IEffectSpriteInfo
    {
        bool OnTopOfCharacter { get; }
        bool Done { get; }

        void NextFrame();
        void Restart();
        void DrawToSpriteBatch(SpriteBatch sb, Rectangle target);
    }
}
