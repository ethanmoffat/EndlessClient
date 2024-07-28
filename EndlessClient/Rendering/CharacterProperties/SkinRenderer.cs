using EndlessClient.Rendering.Metadata.Models;
using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class SkinRenderer : BaseCharacterPropertyRenderer
    {
        private readonly ISpriteSheet _skinSheet;
        private readonly SkinRenderLocationCalculator _skinRenderLocationCalculator;

        public override bool CanRender => true;

        public SkinRenderer(CharacterRenderProperties renderProperties,
                            ISpriteSheet skinSheet)
            : base(renderProperties)
        {
            _skinSheet = skinSheet;

            _skinRenderLocationCalculator = new SkinRenderLocationCalculator(_renderProperties);
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea, WeaponMetadata weaponMetadata)
        {
            var drawLoc = _skinRenderLocationCalculator.CalculateDrawLocationOfCharacterSkin(_skinSheet.SourceRectangle, parentCharacterDrawArea, weaponMetadata.Ranged);
            Render(spriteBatch, _skinSheet, drawLoc);
        }
    }
}