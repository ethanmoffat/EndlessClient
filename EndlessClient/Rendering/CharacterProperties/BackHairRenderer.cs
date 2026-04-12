using EndlessClient.Rendering.Metadata.Models;
using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class BackHairRenderer : BaseCharacterPropertyRenderer
    {
        private readonly ISpriteSheet _backHairSheet;
        private readonly HairRenderLocationCalculator _hairRenderLocationCalculator;

        public override bool CanRender => _backHairSheet.HasTexture && _renderProperties.HairStyle != 0;

        public BackHairRenderer(CharacterRenderProperties renderProperties,
                                ISpriteSheet backHairSheet)
            : base(renderProperties)
        {
            _backHairSheet = backHairSheet;
            _hairRenderLocationCalculator = new HairRenderLocationCalculator(_renderProperties);
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea, WeaponMetadata weaponMetadata)
        {
            var drawLoc = _hairRenderLocationCalculator.CalculateDrawLocationOfCharacterHair(_backHairSheet.SourceRectangle, parentCharacterDrawArea, weaponMetadata.Ranged);
            Render(spriteBatch, _backHairSheet, drawLoc);
        }
    }
}
