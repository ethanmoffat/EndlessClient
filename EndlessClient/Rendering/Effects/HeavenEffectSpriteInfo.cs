// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
    public class HeavenEffectSpriteInfo : FlickeringEffectSpriteInfo
    {
        private int _whichFrame;

        public HeavenEffectSpriteInfo(bool onTopOfCharacter, int alpha, Texture2D texture)
            : base(200, 400, 5, 3, onTopOfCharacter, alpha, texture)
        {
            _whichFrame = _numberOfFrames - 2;
        }

        protected override void FlickerTimeChanged()
        {
            //when the flicker time changes, toggle between using the second to last and last frames
            _whichFrame =
                _whichFrame == _numberOfFrames - 2
                    ? _numberOfFrames - 1
                    : _numberOfFrames - 2;
        }

        protected override Rectangle GetFrameSourceRectangle()
        {
            var frameWidth = _graphic.Width / _numberOfFrames;
            return new Rectangle(_whichFrame * frameWidth, 0, frameWidth, _graphic.Height);
        }

        protected override Vector2 GetDrawLocation(Rectangle textureSourceRectangle, Rectangle targetActorRectangle)
        {
            var targetX = targetActorRectangle.X + (targetActorRectangle.Width - textureSourceRectangle.Width) / 2;
            var targetY = targetActorRectangle.Bottom - textureSourceRectangle.Height + 30; //arbitrary additional 30px offset

            return new Vector2(targetX, targetY);
        }
    }
}
