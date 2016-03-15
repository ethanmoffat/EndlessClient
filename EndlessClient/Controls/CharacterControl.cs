// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering;
using EOLib;
using EOLib.Data.BLL;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using XNAControls;

namespace EndlessClient.Controls
{
	public class CharacterControl : XNAControl
	{
		public ICharacterRenderProperties CharacterRenderProperties
		{
			get { return RenderProperties; }
		}

		private ICharacterRenderProperties RenderProperties
		{
			get { return _characterRenderer.RenderProperties; }
			set { _characterRenderer.RenderProperties = value; }
		}

		private readonly ICharacterRenderer _characterRenderer;

		public CharacterControl()
		{
			_characterRenderer = new CharacterRenderer((EOGame)Game, GetDefaultProperties());
			_setSize(99, 123);
			
			((DrawableGameComponent)_characterRenderer).DrawOrder = DrawOrder;
			Game.Components.Add(_characterRenderer);
		}

		public override void Update(GameTime gameTime)
		{
			if (!ShouldUpdate())
				return;

			((DrawableGameComponent)_characterRenderer).DrawOrder = DrawOrder;
			_characterRenderer.SetAbsoluteScreenPosition(DrawAreaWithOffset.X + 34, DrawAreaWithOffset.Y + 25);

			var currentState = Mouse.GetState();
			if (((currentState.LeftButton == ButtonState.Released && PreviousMouseState.LeftButton == ButtonState.Pressed) ||
				(currentState.RightButton == ButtonState.Released && PreviousMouseState.RightButton == ButtonState.Pressed)) &&
				DrawAreaWithOffset.ContainsPoint(currentState.X, currentState.Y))
			{
				RenderProperties =
					RenderProperties.WithDirection(
						(EODirection) (((int) RenderProperties.Direction + 1)%4));
			}

			base.Update(gameTime);
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
			return new CharacterRenderProperties()
				.WithHairStyle(1)
				.WithHairColor(1);
		}

		public override void Close()
		{
			Game.Components.Remove(_characterRenderer);
			base.Close();
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
