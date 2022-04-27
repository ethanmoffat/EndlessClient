using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class HairRenderer : BaseCharacterPropertyRenderer
    {
        private readonly ISpriteSheet _hairSheet;
        private readonly HairRenderLocationCalculator _hairRenderLocationCalculator;

        public override bool CanRender => _hairSheet.HasTexture && _renderProperties.HairStyle != 0;

        public HairRenderer(CharacterRenderProperties renderProperties,
                            ISpriteSheet hairSheet)
            : base(renderProperties)
        {
            _hairSheet = hairSheet;
            _hairRenderLocationCalculator = new HairRenderLocationCalculator(_renderProperties);
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            var drawLoc = _hairRenderLocationCalculator.CalculateDrawLocationOfCharacterHair(_hairSheet.SourceRectangle, parentCharacterDrawArea);
            Render(spriteBatch, _hairSheet, drawLoc);
        }
    }
}
