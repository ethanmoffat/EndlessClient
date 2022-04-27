using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class EmoteRenderer : BaseCharacterPropertyRenderer
    {
        private readonly ISpriteSheet _emoteSheet;
        private readonly ISpriteSheet _skinSheet;
        private readonly SkinRenderLocationCalculator _skinRenderLocationCalculator;

        public override bool CanRender => _renderProperties.EmoteFrame > 0;

        protected override bool ShouldFlip => false;

        public EmoteRenderer(CharacterRenderProperties renderProperties,
                             ISpriteSheet emoteSheet,
                             ISpriteSheet skinSheet)
            : base(renderProperties)
        {
            _emoteSheet = emoteSheet;
            _skinSheet = skinSheet;
            _skinRenderLocationCalculator = new SkinRenderLocationCalculator(_renderProperties);
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            var skinLoc = _skinRenderLocationCalculator.CalculateDrawLocationOfCharacterSkin(_skinSheet.SourceRectangle, parentCharacterDrawArea);
            var emotePos = new Vector2(skinLoc.X - 15, parentCharacterDrawArea.Y - _emoteSheet.SheetTexture.Height);
            Render(spriteBatch, _emoteSheet, emotePos, 128);
        }
    }
}
