// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
	public class AuraEffectSpriteInfo : EffectSpriteInfo
	{
		private readonly Random _gen;
		private int _msToWait;
		private DateTime _lastDrawTime;

		public AuraEffectSpriteInfo(Texture2D auraTexture)
			: base(5, 2, true, 128, auraTexture)
		{
			_gen = new Random();

			_msToWait = GetMillisecondsBetweenDraws();
			_lastDrawTime = DateTime.Now;
		}

		public override void DrawToSpriteBatch(SpriteBatch sb, Rectangle targetRectangle)
		{
			//draw only every certain number of milliseconds, for flicker effect
			if ((DateTime.Now - _lastDrawTime).TotalMilliseconds > _msToWait)
			{
				base.DrawToSpriteBatch(sb, targetRectangle);
			}

			//keep drawing for 100ms (so it doesn't immediately disappear)
			if ((DateTime.Now - _lastDrawTime).TotalMilliseconds > _msToWait + 100)
			{
				_lastDrawTime = DateTime.Now;
				_msToWait = GetMillisecondsBetweenDraws();
			}
		}

		protected override Rectangle GetFrameSourceRectangle()
		{
			var frameWidth = _graphic.Width / _numberOfFrames;
			return new Rectangle((_numberOfFrames - 1) * frameWidth, 0, frameWidth, _graphic.Height);
		}

		private int GetMillisecondsBetweenDraws()
		{
			return _gen.Next(100, 300);
		}
	}
}
