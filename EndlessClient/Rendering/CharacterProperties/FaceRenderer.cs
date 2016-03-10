// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Sprites;
using EOLib.Data.BLL;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
	public class FaceRenderer : ICharacterPropertyRenderer
	{
		private readonly SpriteBatch _spriteBatch;
		private readonly ICharacterRenderProperties _renderProperties;
		private readonly ISpriteSheet _faceSheet;

		public FaceRenderer(SpriteBatch spriteBatch, ICharacterRenderProperties renderProperties, ISpriteSheet faceSheet)
		{
			_spriteBatch = spriteBatch;
			_renderProperties = renderProperties;
			_faceSheet = faceSheet;
		}

		public void Render(Rectangle parentCharacterDrawArea)
		{
			throw new System.NotImplementedException();
		}
	}
}
