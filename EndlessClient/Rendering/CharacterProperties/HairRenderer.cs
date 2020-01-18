// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class HairRenderer : ICharacterPropertyRenderer
    {
        private readonly ICharacterRenderProperties _renderProperties;
        private readonly ISpriteSheet _hairSheet;
        private readonly HairRenderLocationCalculator _hairRenderLocationCalculator;

        public bool CanRender => _hairSheet.HasTexture && _renderProperties.HairStyle != 0;

        public HairRenderer(ICharacterRenderProperties renderProperties,
                            ISpriteSheet hairSheet)
        {
            _renderProperties = renderProperties;
            _hairSheet = hairSheet;

            _hairRenderLocationCalculator = new HairRenderLocationCalculator(_renderProperties);
        }

        public void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            var drawLoc = _hairRenderLocationCalculator.CalculateDrawLocationOfCharacterHair(_hairSheet.SourceRectangle, parentCharacterDrawArea);

            spriteBatch.Draw(_hairSheet.SheetTexture, drawLoc, _hairSheet.SourceRectangle, Color.White, 0.0f, Vector2.Zero, 1.0f,
                             _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                             0.0f);
        }
    }
}
