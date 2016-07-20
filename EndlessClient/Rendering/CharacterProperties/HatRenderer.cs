// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib;
using EOLib.Domain.Character;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class HatRenderer : ICharacterPropertyRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly ICharacterRenderProperties _renderProperties;
        private readonly Texture2D _hatTexture;
        private readonly HairRenderLocationCalculator _hairRenderLocationCalculator;

        public HatRenderer(SpriteBatch spriteBatch,
                           ICharacterRenderProperties renderProperties,
                           Texture2D hatTexture,
                           IPubFile<EIFRecord> itemFile)
        {
            _spriteBatch = spriteBatch;
            _renderProperties = renderProperties;
            _hatTexture = hatTexture;

            _hairRenderLocationCalculator = new HairRenderLocationCalculator(itemFile, _renderProperties);
        }

        public void Render(Rectangle parentCharacterDrawArea)
        {
            var offsets = _hairRenderLocationCalculator.CalculateDrawLocationOfCharacterHair(parentCharacterDrawArea);
            var flippedOffset = _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? -2 : 0;
            var drawLoc = new Vector2(offsets.X + flippedOffset, offsets.Y - 3);

            _spriteBatch.Draw(_hatTexture, drawLoc, null, Color.White, 0.0f, Vector2.Zero, 1.0f,
                              _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                              0.0f);
        }
    }
}
