﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
    public class BottomAlignedEffectSpriteInfo : EffectSpriteInfo
    {
        public BottomAlignedEffectSpriteInfo(int numberOfFrames,
                                          int repeats,
                                          bool onTopOfCharacter,
                                          int alpha,
                                          Texture2D graphic)
            : base(numberOfFrames, repeats, onTopOfCharacter, alpha, graphic) { }

        protected override Vector2 GetDrawLocation(Rectangle textureSourceRectangle, Rectangle targetActorRectangle)
        {
            var x = targetActorRectangle.X + (targetActorRectangle.Width - textureSourceRectangle.Width) / 2;
            var y = targetActorRectangle.Bottom - textureSourceRectangle.Height;

            return new Vector2(x, y);
        }
    }
}
