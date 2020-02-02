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
    public class SkinRenderer : BaseCharacterPropertyRenderer
    {
        private readonly ISpriteSheet _skinSheet;
        private readonly bool _weaponIsRanged;
        private readonly SkinRenderLocationCalculator _skinRenderLocationCalculator;

        public override bool CanRender => true;

        public SkinRenderer(ICharacterRenderProperties renderProperties,
                            ISpriteSheet skinSheet,
                            bool weaponIsRanged)
            : base(renderProperties)
        {
            _skinSheet = skinSheet;
            _weaponIsRanged = weaponIsRanged;

            _skinRenderLocationCalculator = new SkinRenderLocationCalculator(_renderProperties, _weaponIsRanged);
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            var drawLoc = _skinRenderLocationCalculator.CalculateDrawLocationOfCharacterSkin(_skinSheet.SourceRectangle, parentCharacterDrawArea);
            Render(spriteBatch, _skinSheet, drawLoc);
        }
    }
}
