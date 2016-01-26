// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
	public class EffectSpriteInfo
	{
		public bool OnTopOfCharacter { get; private set; }

		protected readonly int _numberOfFrames;
		protected readonly int _repeats;
		protected readonly int _alpha;
		protected readonly Texture2D _graphic;

		protected int _currentFrame;
		protected int _iterations;

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

			var targetX = targetRectangle.X + (targetRectangle.Width - sourceRect.Width) / 2;
			var targetY = targetRectangle.Y + (targetRectangle.Height - sourceRect.Height) / 2;

			sb.Draw(_graphic, new Vector2(targetX, targetY), sourceRect, Color.FromNonPremultiplied(255, 255, 255, _alpha));
		}

		protected virtual Rectangle GetFrameSourceRectangle()
		{
			var frameWidth = _graphic.Width / _numberOfFrames;
			return new Rectangle(_currentFrame * frameWidth, 0, frameWidth, _graphic.Height);
		}
	}
}
