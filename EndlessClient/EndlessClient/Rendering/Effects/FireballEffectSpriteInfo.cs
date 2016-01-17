using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
	public class FireballEffectSpriteInfo : EffectSpriteInfo
	{
		public FireballEffectSpriteInfo(int numberOfFrames,
									  int repeats,
									  bool onTopOfCharacter,
									  int alpha,
									  Texture2D graphic)
			: base(numberOfFrames, repeats, onTopOfCharacter, alpha, graphic)
		{
		}

		public override void DrawToSpriteBatch(SpriteBatch sb, Rectangle targetRectangle)
		{
			var frameWidth = _graphic.Width / _numberOfFrames;
			var sourceRect = new Rectangle(_currentFrame * frameWidth, 0, frameWidth, _graphic.Height);

			var targetX = targetRectangle.X + (targetRectangle.Width - sourceRect.Width) / 2;
			var targetY = targetRectangle.Y - (_numberOfFrames - _currentFrame) * sourceRect.Height/3;

			sb.Draw(_graphic, new Vector2(targetX, targetY), sourceRect, Color.FromNonPremultiplied(255, 255, 255, _alpha));
		}
	}
}
