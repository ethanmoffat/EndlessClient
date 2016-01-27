// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
	public class EffectSpriteInfo : IEffectSpriteInfo
	{
		protected readonly int _numberOfFrames;
		protected readonly int _repeats;
		protected readonly int _alpha;
		protected readonly Texture2D _graphic;

		protected int _currentFrame;
		protected int _iterations;

		public bool OnTopOfCharacter { get; private set; }
		public bool Done { get { return _iterations == _repeats; } }

		public EffectSpriteInfo(int numberOfFrames,
								int repeats,
								bool onTopOfCharacter,
								int alpha,
								Texture2D graphic)
		{
			_numberOfFrames = numberOfFrames;
			_repeats = repeats;
			OnTopOfCharacter = onTopOfCharacter;
			_alpha = alpha;
			_graphic = graphic;
		}

		public void NextFrame()
		{
			_currentFrame++;
			if (_currentFrame >= _numberOfFrames)
			{
				_currentFrame = 0;
				_iterations++;
			}
		}

		public void Restart()
		{
			_currentFrame = 0;
			_iterations = 0;
		}

		public virtual void DrawToSpriteBatch(SpriteBatch sb, Rectangle targetRectangle)
		{
			var sourceRect = GetFrameSourceRectangle();
			var drawLocation = GetDrawLocation(sourceRect, targetRectangle);

			sb.Draw(_graphic, drawLocation, sourceRect, Color.FromNonPremultiplied(255, 255, 255, _alpha));
		}

		protected virtual Rectangle GetFrameSourceRectangle()
		{
			var frameWidth = _graphic.Width / _numberOfFrames;
			return new Rectangle(_currentFrame * frameWidth, 0, frameWidth, _graphic.Height);
		}

		protected virtual Vector2 GetDrawLocation(Rectangle textureSourceRectangle, Rectangle targetActorRectangle)
		{
			var targetX = targetActorRectangle.X + (targetActorRectangle.Width - textureSourceRectangle.Width) / 2;
			var targetY = targetActorRectangle.Y + (targetActorRectangle.Height - textureSourceRectangle.Height) / 2;

			return new Vector2(targetX, targetY);
		}
	}
}
