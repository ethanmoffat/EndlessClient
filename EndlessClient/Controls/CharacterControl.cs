// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering;
using EOLib.Data.BLL;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient.Controls
{
	public class CharacterControl : XNAControl
	{
		public ICharacterRenderProperties RenderProperties
		{
			get { return _characterRenderer.RenderProperties; }
			protected set { _characterRenderer.RenderProperties = value; }
		}

		private readonly ICharacterRenderer _characterRenderer;

		public CharacterControl()
		{
			_characterRenderer = new CharacterRenderer((EOGame)Game, GetDefaultProperties());
			_characterRenderer.Initialize();

			_setSize(99, 123);
		}

		public override void Update(GameTime gameTime)
		{
			if (!ShouldUpdate())
				return;

			_characterRenderer.SetAbsoluteScreenPosition(DrawAreaWithOffset.X + 34, DrawAreaWithOffset.Y + 25);
			
			_characterRenderer.Update(gameTime);

			base.Update(gameTime);
		}

		public override void Draw(GameTime gameTime)
		{
			_characterRenderer.Draw(gameTime);

			base.Draw(gameTime);
		}

		public void NextGender()
		{
			RenderProperties = RenderProperties.WithGender((byte)((RenderProperties.Gender + 1) % 2));
		}

		public void NextRace()
		{
			RenderProperties = RenderProperties.WithRace((byte)((RenderProperties.Race + 1) % 6));
		}

		public void NextHairStyle()
		{
			RenderProperties = RenderProperties.WithHairStyle((byte)((RenderProperties.HairStyle + 1) % 21));
		}

		public void NextHairColor()
		{
			RenderProperties = RenderProperties.WithHairColor((byte)((RenderProperties.HairColor + 1) % 10));
		}

		private static ICharacterRenderProperties GetDefaultProperties()
		{
			return new CharacterRenderProperties().WithHairStyle(1);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_characterRenderer.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
