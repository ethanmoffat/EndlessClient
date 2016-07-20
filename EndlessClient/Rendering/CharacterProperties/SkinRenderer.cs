// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class SkinRenderer : ICharacterPropertyRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly ICharacterRenderProperties _renderProperties;
        private readonly ISpriteSheet _skinSheet;
        private readonly SkinRenderLocationCalculator _skinRenderLocationCalculator;

        public SkinRenderer(SpriteBatch spriteBatch,
                           ICharacterRenderProperties renderProperties,
                           ISpriteSheet skinSheet,
                           IPubFile<EIFRecord> itemFile)
        {
            _spriteBatch = spriteBatch;
            _renderProperties = renderProperties;
            _skinSheet = skinSheet;
            
            _skinRenderLocationCalculator = new SkinRenderLocationCalculator(_renderProperties, itemFile);
        }

        public void Render(Rectangle parentCharacterDrawArea)
        {
            //todo: I most likely screwed something up when re-implementing this so it needs to be verified for all states!

            var drawLoc = _skinRenderLocationCalculator.CalculateDrawLocationOfCharacterSkin(parentCharacterDrawArea);
            _spriteBatch.Draw(_skinSheet.SheetTexture, drawLoc, _skinSheet.SourceRectangle, Color.White, 0.0f, Vector2.Zero, 1.0f,
                              _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                              0.0f);
        }
    }
}
