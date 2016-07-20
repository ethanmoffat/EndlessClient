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
    public class HairRenderer : ICharacterPropertyRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly ICharacterRenderProperties _renderProperties;
        private readonly Texture2D _hairTexture;
        private readonly HairRenderLocationCalculator _hairRenderLocationCalculator;

        public HairRenderer(SpriteBatch spriteBatch,
                            ICharacterRenderProperties renderProperties,
                            Texture2D hairTexture,
                            IPubFile<EIFRecord> itemFile)
        {
            _spriteBatch = spriteBatch;
            _renderProperties = renderProperties;
            _hairTexture = hairTexture;

            _hairRenderLocationCalculator = new HairRenderLocationCalculator(itemFile, _renderProperties);
        }

        public void Render(Rectangle parentCharacterDrawArea)
        {
            var drawLoc = _hairRenderLocationCalculator.CalculateDrawLocationOfCharacterHair(parentCharacterDrawArea);

            _spriteBatch.Draw(_hairTexture, drawLoc, null, Color.White, 0.0f, Vector2.Zero, 1.0f,
                              _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                              0.0f);
        }
    }
}
