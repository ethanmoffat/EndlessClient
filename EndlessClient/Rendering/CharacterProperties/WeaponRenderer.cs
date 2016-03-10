// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Data.BLL;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
	public class WeaponRenderer : ICharacterPropertyRenderer
	{
		private readonly SpriteBatch _spriteBatch;
		private readonly ICharacterRenderProperties _renderProperties;
		private readonly Texture2D _weaponTexture;

		public WeaponRenderer(SpriteBatch spriteBatch, ICharacterRenderProperties renderProperties, Texture2D weaponTexture)
		{
			_spriteBatch = spriteBatch;
			_renderProperties = renderProperties;
			_weaponTexture = weaponTexture;
		}

		public void Render(Rectangle parentCharacterDrawArea)
		{
			throw new NotImplementedException();
		}
	}
}
