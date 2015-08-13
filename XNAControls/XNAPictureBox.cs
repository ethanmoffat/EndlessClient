using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XNAControls
{
	public enum StretchMode
	{
		CenterInFrame,
		Stretch
	}

	public class XNAPictureBox : XNAControl
	{
		public StretchMode StretchMode { get; set; }

		public Texture2D Texture { get; set; }

		public XNAPictureBox(Rectangle area)
			: base(new Vector2(area.X, area.Y), area)
		{
		}

		public override void Draw(GameTime gameTime)
		{
			if (!Visible)
				return;

			if (Texture != null)
			{
				SpriteBatch.Begin();
				switch (StretchMode)
				{
					case StretchMode.CenterInFrame:
						SpriteBatch.Draw(Texture,
							new Rectangle(DrawAreaWithOffset.X + DrawArea.Width / 2 - Texture.Width / 2,
								DrawAreaWithOffset.Y + DrawArea.Height / 2 - Texture.Width / 2,
								Texture.Width,
								Texture.Height),
							Color.White);
						break;

					case StretchMode.Stretch:
						SpriteBatch.Draw(Texture, new Rectangle(DrawAreaWithOffset.X, DrawAreaWithOffset.Y, DrawArea.Width, DrawArea.Height), Color.White);
						break;
				}
				SpriteBatch.End();
			}

			base.Draw(gameTime);
		}
	}
}
