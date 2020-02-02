// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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
        private readonly bool _weaponIsRanged;
        private readonly SkinRenderLocationCalculator _skinRenderLocationCalculator;

        public override bool CanRender => _renderProperties.IsActing(CharacterActionState.Emote) &&
                                 _renderProperties.EmoteFrame > 0;

        public EmoteRenderer(ICharacterRenderProperties renderProperties,
                             ISpriteSheet emoteSheet,
                             ISpriteSheet skinSheet,
                             bool weaponIsRanged)
            : base(renderProperties)
        {
            _emoteSheet = emoteSheet;
            _skinSheet = skinSheet;
            _weaponIsRanged = weaponIsRanged;
            _skinRenderLocationCalculator = new SkinRenderLocationCalculator(_renderProperties, _weaponIsRanged);
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            var skinLoc = _skinRenderLocationCalculator.CalculateDrawLocationOfCharacterSkin(_skinSheet.SourceRectangle, parentCharacterDrawArea);
            var emotePos = new Vector2(skinLoc.X - 15, parentCharacterDrawArea.Y - _emoteSheet.SheetTexture.Height + 10);
            Render(spriteBatch, _emoteSheet, emotePos, 128);
        }
    }
}
