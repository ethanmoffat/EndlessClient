// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Sprites;
using EOLib.Data.BLL;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
	public class EmoteRenderer : ICharacterPropertyRenderer
	{
		private readonly SpriteBatch _spriteBatch;
		private readonly ICharacterRenderProperties _renderProperties;
		private readonly ISpriteSheet _emoteSheet;

		public EmoteRenderer(SpriteBatch spriteBatch, ICharacterRenderProperties renderProperties, ISpriteSheet emoteSheet)
		{
			_spriteBatch = spriteBatch;
			_renderProperties = renderProperties;
			_emoteSheet = emoteSheet;
		}

		public void Render(Rectangle parentCharacterDrawArea)
		{
			throw new System.NotImplementedException();
		}
	}
}
