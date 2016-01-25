// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Effects
{
	public class EnergyBallEffectSpriteInfo : GenericMovingEffectSpriteInfo
	{
		public EnergyBallEffectSpriteInfo(int numberOfFrames,
										  int repeats,
										  bool onTopOfCharacter,
										  int alpha,
										  Texture2D graphic)
			: base(numberOfFrames, repeats, onTopOfCharacter, alpha, graphic) { }

		protected override float GetTargetXForFrame(Rectangle targetRectangle)
		{
			return targetRectangle.X + (targetRectangle.Width - SourceRectangle.Width) / 2;
		}

		protected override float GetTargetYForFrame(Rectangle targetRectangle)
		{
			return targetRectangle.Y - _currentFrame * SourceRectangle.Height / 3;
		}
	}
}
