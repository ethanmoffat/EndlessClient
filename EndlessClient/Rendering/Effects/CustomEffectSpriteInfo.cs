using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
    public class CustomEffectSpriteInfo : EffectSpriteInfo
    {
        public CustomEffectSpriteInfo(int numberOfFrames, int repeats, bool onTopOfCharacter, int alpha, Texture2D graphic)
            : base(numberOfFrames, repeats, onTopOfCharacter, alpha, graphic)
        {
        }

        protected override Vector2 GetDrawLocation(Rectangle textureSourceRectangle, Rectangle targetActorRectangle)
        {
            var targetX = targetActorRectangle.X + (targetActorRectangle.Width - textureSourceRectangle.Width) / 2 - targetActorRectangle.Width / 2;
            var targetY = targetActorRectangle.Y - textureSourceRectangle.Height;

            return new Vector2(targetX, targetY);
        }
    }
}
