// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.UIControls
{
	public class DisposableButton : XNAButton
	{
		private readonly Texture2D _outTexture;
		private readonly Texture2D _overTexture;

		public DisposableButton(Vector2 location, Texture2D outTexture, Texture2D overTexture)
			: base(new[] {outTexture, overTexture}, location)
		{
			_outTexture = outTexture;
			_overTexture = overTexture;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_outTexture.Dispose();
				_overTexture.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
