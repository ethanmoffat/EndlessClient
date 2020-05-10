using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
    public class EnergyBallEffectSpriteInfo : EffectSpriteInfo
    {
        public EnergyBallEffectSpriteInfo(int numberOfFrames,
                                          int repeats,
                                          bool onTopOfCharacter,
                                          int alpha,
                                          Texture2D graphic)
            : base(numberOfFrames, repeats, onTopOfCharacter, alpha, graphic) { }

        protected override Vector2 GetDrawLocation(Rectangle textureSourceRectangle, Rectangle targetActorRectangle)
        {
            var x = targetActorRectangle.X + (targetActorRectangle.Width - textureSourceRectangle.Width) / 2;
            var y = targetActorRectangle.Y - _currentFrame * textureSourceRectangle.Height / 3;

            return new Vector2(x, y);
        }
    }
}
