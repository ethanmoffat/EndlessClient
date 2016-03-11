// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Data.BLL;
using EOLib.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
	public class FaceRenderer : ICharacterPropertyRenderer
	{
		private readonly SpriteBatch _spriteBatch;
		private readonly ICharacterRenderProperties _renderProperties;
		private readonly ISpriteSheet _faceSheet;
		private readonly SkinRenderLocationCalculator _skinRenderLocationCalculator;

		public FaceRenderer(SpriteBatch spriteBatch,
							ICharacterRenderProperties renderProperties,
							ISpriteSheet faceSheet,
							IDataFile<ItemRecord> itemFile)
		{
			_spriteBatch = spriteBatch;
			_renderProperties = renderProperties;
			_faceSheet = faceSheet;
			_skinRenderLocationCalculator = new SkinRenderLocationCalculator(_renderProperties, itemFile);
		}

		public void Render(Rectangle parentCharacterDrawArea)
		{
			if (!_renderProperties.IsFacing(EODirection.Down, EODirection.Right))
				return;

			var skinLoc = _skinRenderLocationCalculator.CalculateDrawLocationOfCharacterSkin(parentCharacterDrawArea);
			var facePos = new Vector2(skinLoc.X + (_renderProperties.IsFacing(EODirection.Down) ? 2 : 3),
									  skinLoc.Y + (_renderProperties.Gender == 0 ? 2 : 0));

			_spriteBatch.Draw(_faceSheet.SheetTexture, facePos, _faceSheet.SourceRectangle, Color.White, 0f, Vector2.Zero, 1f,
							  _renderProperties.IsFacing(EODirection.Right) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
							  0f);
		}
	}
}
