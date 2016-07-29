// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class EmoteRenderer : ICharacterPropertyRenderer
    {
        private readonly ISpriteSheet _emoteSheet;
        private readonly SkinRenderLocationCalculator _skinRenderLocationCalculator;

        public EmoteRenderer(ICharacterRenderProperties renderProperties,
                             ISpriteSheet emoteSheet,
                             IPubFile<EIFRecord> itemFile)
        {
            _emoteSheet = emoteSheet;
            _skinRenderLocationCalculator = new SkinRenderLocationCalculator(renderProperties, itemFile);
        }

        public void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            var skinLoc = _skinRenderLocationCalculator.CalculateDrawLocationOfCharacterSkin(parentCharacterDrawArea);
            var emotePos = new Vector2(skinLoc.X - 15, parentCharacterDrawArea.Y - _emoteSheet.SheetTexture.Height + 10);

            spriteBatch.Draw(_emoteSheet.SheetTexture,
                             emotePos,
                             _emoteSheet.SourceRectangle,
                             Color.FromNonPremultiplied(0xff, 0xff, 0xff, 128));
        }
    }
}
