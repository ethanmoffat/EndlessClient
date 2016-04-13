// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XNAControls;

namespace EndlessClient.UIControls
{
	public class PictureBox : XNAControl
	{
		private Texture2D _displayPicture;

		public PictureBox(Texture2D displayPicture, XNAControl parent = null)
			: base(null, null, parent)
		{
			SetNewPicture(displayPicture);
			if (parent == null)
				Game.Components.Add(this);
		}

		public void SetNewPicture(Texture2D displayPicture)
		{
			_displayPicture = displayPicture;
			_setSize(displayPicture.Width, displayPicture.Height);
		}

		public override void Draw(GameTime gameTime)
		{
			SpriteBatch.Begin();
			SpriteBatch.Draw(_displayPicture, DrawAreaWithOffset, Color.White);
			SpriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
