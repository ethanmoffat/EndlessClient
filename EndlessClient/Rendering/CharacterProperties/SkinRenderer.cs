// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Sprites;
using EOLib.Data.BLL;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
	public class SkinRenderer : ICharacterPropertyRenderer
	{
		private readonly SpriteBatch _spriteBatch;
		private readonly ICharacterRenderProperties _renderProperties;
		private readonly ISpriteSheet _skinSheet;

		public SkinRenderer(SpriteBatch spriteBatch, ICharacterRenderProperties renderProperties, ISpriteSheet skinSheet)
		{
			_spriteBatch = spriteBatch;
			_renderProperties = renderProperties;
			_skinSheet = skinSheet;
		}

		public void Render(Rectangle parentCharacterDrawArea)
		{
			throw new System.NotImplementedException();
		}
	}
}
