// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
	public class AuraEffectSpriteInfo : FlickeringEffectSpriteInfo
	{
		public AuraEffectSpriteInfo(Texture2D texture)
			: base(100, 300, 5, 3, true, 128, texture) { }

		protected override void FlickerTimeChanged() { }

		protected override Rectangle GetFrameSourceRectangle()
		{
			var frameWidth = _graphic.Width / _numberOfFrames;
			return new Rectangle((_numberOfFrames - 1) * frameWidth, 0, frameWidth, _graphic.Height);
		}
	}
}
