// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
	public abstract class GenericMovingEffectSpriteInfo : EffectSpriteInfo
	{
		protected GenericMovingEffectSpriteInfo(int numberOfFrames,
												int repeats,
												bool onTopOfCharacter,
												int alpha,
												Texture2D graphic) 
			: base(numberOfFrames, repeats, onTopOfCharacter, alpha, graphic) { }

		public override void DrawToSpriteBatch(SpriteBatch sb, Rectangle targetRectangle)
		{
			var targetX = GetTargetXForFrame(targetRectangle);
			var targetY = GetTargetYForFrame(targetRectangle);

			sb.Draw(_graphic, new Vector2(targetX, targetY), SourceRectangle, Color.FromNonPremultiplied(255, 255, 255, _alpha));
		}

		protected abstract float GetTargetXForFrame(Rectangle targetRectangle);
		protected abstract float GetTargetYForFrame(Rectangle targetRectangle);

		private int FrameWidth { get { return _graphic.Width / _numberOfFrames; } }

		protected Rectangle SourceRectangle
		{
			get { return new Rectangle(_currentFrame * FrameWidth, 0, FrameWidth, _graphic.Height); }
		}
	}
}
