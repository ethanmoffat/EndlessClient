using EndlessClient.Content;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class HatRenderer : BaseCharacterPropertyRenderer
    {
        private readonly IShaderProvider _shaderProvider;
        private readonly ISpriteSheet _hatSheet;
        private readonly ISpriteSheet _hairSheet;
        private readonly HairRenderLocationCalculator _hairRenderLocationCalculator;

        public override bool CanRender => _hatSheet.HasTexture && _renderProperties.HatGraphic != 0;

        public HatRenderer(IShaderProvider shaderProvider,
                           ICharacterRenderProperties renderProperties,
                           ISpriteSheet hatSheet,
                           ISpriteSheet hairSheet)
            : base(renderProperties)
        {
            _shaderProvider = shaderProvider;
            _hatSheet = hatSheet;
            _hairSheet = hairSheet;

            _hairRenderLocationCalculator = new HairRenderLocationCalculator(_renderProperties);
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            var hairDrawLoc = _hairRenderLocationCalculator.CalculateDrawLocationOfCharacterHair(_hairSheet.SourceRectangle, parentCharacterDrawArea);
            var offsets = GetOffsets();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                ApplyHairClipShader();

            Render(spriteBatch, _hatSheet, hairDrawLoc + offsets);
        }

        private Vector2 GetOffsets()
        {
            var xOff = 0f;
            var yOff = -3f;

            var flippedOffset = _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? -2 : 0;

            return new Vector2(xOff + flippedOffset, yOff);
        }

        private void ApplyHairClipShader()
        {
            _shaderProvider.Shaders[ShaderRepository.HairClip].CurrentTechnique.Passes[0].Apply();
        }
    }
}
