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
    public class HatRenderer : ICharacterPropertyRenderer
    {
        private readonly ICharacterRenderProperties _renderProperties;
        private readonly ISpriteSheet _hatSheet;
        private readonly ISpriteSheet _hairSheet;
        private readonly HairRenderLocationCalculator _hairRenderLocationCalculator;

        public bool CanRender => _hatSheet.HasTexture && _renderProperties.HatGraphic != 0;

        public HatRenderer(ICharacterRenderProperties renderProperties,
                           ISpriteSheet hatSheet,
                           ISpriteSheet hairSheet)
        {
            _renderProperties = renderProperties;
            _hatSheet = hatSheet;
            _hairSheet = hairSheet;

            _hairRenderLocationCalculator = new HairRenderLocationCalculator(_renderProperties);
        }

        public void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            var offsets = _hairRenderLocationCalculator.CalculateDrawLocationOfCharacterHair(_hairSheet.SourceRectangle, parentCharacterDrawArea);
            var flippedOffset = _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? -2 : 0;
            var drawLoc = new Vector2(offsets.X + flippedOffset, offsets.Y - 3);

            spriteBatch.Draw(_hatSheet.SheetTexture, drawLoc, _hatSheet.SourceRectangle, Color.White, 0.0f, Vector2.Zero, 1.0f,
                             _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                             0.0f);
        }
    }
}
