// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Content;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class HatRenderer : BaseCharacterPropertyRenderer
    {
        private readonly IShaderProvider _shaderProvider;
        private readonly ISpriteSheet _hatSheet;
        private readonly ISpriteSheet _hairSheet;
        private readonly HairRenderLocationCalculator _hairRenderLocationCalculator;

        public override  bool CanRender => _hatSheet.HasTexture && _renderProperties.HatGraphic != 0;

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
            var offsets = _hairRenderLocationCalculator.CalculateDrawLocationOfCharacterHair(_hairSheet.SourceRectangle, parentCharacterDrawArea);
            var flippedOffset = _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? -2 : 0;
            var drawLoc = new Vector2(offsets.X + flippedOffset, offsets.Y - 3);

#if LINUX
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, effect: _shaderProvider.Shaders[ShaderRepository.HairClip]);
#endif

            Render(spriteBatch, _hatSheet, drawLoc);

#if LINUX
            spriteBatch.End();
            spriteBatch.Begin();
#endif
        }
    }
}
