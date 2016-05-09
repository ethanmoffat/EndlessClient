// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.UIControls
{
	public class TimeLabel : XNALabel
	{
		private DateTime _lastUpdateTime;

		public TimeLabel(IClientWindowSizeProvider windowSizeProvider)
			: base(GetPositionBasedOnWindowSize(windowSizeProvider), Constants.FontSize07)
		{
			_lastUpdateTime = DateTime.Now;
		}

		public override void Update(GameTime gameTime)
		{
			if (DateTime.Now.Second != _lastUpdateTime.Second)
			{
				Text = string.Format("{0,2:D2}:{1,2:D2}:{2,2:D2}",
					DateTime.Now.Hour,
					DateTime.Now.Minute,
					DateTime.Now.Second);

				_lastUpdateTime = DateTime.Now;
			}

			base.Update(gameTime);
		}

		private static Rectangle GetPositionBasedOnWindowSize(IClientWindowSizeProvider windowSizeProvider)
		{
			//original location: 558, 455
			var xLoc = windowSizeProvider.Width - 82;
			var yLoc = windowSizeProvider.Height - 25;

			return new Rectangle(xLoc, yLoc, 1, 1);
		}
	}
}
