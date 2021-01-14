using System;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class WeaponRenderer : BaseCharacterPropertyRenderer
    {
        private readonly ISpriteSheet _weaponSheet;

        public override bool CanRender => _weaponSheet.HasTexture && _renderProperties.WeaponGraphic != 0;

        public WeaponRenderer(ICharacterRenderProperties renderProperties,
                              ISpriteSheet weaponSheet)
            : base(renderProperties)
        {
            _weaponSheet = weaponSheet;
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            if (_renderProperties.IsActing(CharacterActionState.Sitting, CharacterActionState.SpellCast))
                return;

            var offsets = GetOffsets(parentCharacterDrawArea);
            var drawLoc = new Vector2(parentCharacterDrawArea.X + offsets.X, parentCharacterDrawArea.Y + offsets.Y);
            Render(spriteBatch, _weaponSheet, drawLoc);
        }

        private Vector2 GetOffsets(Rectangle parentCharacterDrawArea)
        {
            float resX, resY;

            resX = -(float)Math.Floor(Math.Abs((float)_weaponSheet.SourceRectangle.Width - parentCharacterDrawArea.Width) / 2);
            resY = -(float)Math.Floor(Math.Abs((float)_weaponSheet.SourceRectangle.Height - parentCharacterDrawArea.Height) / 2) - 5;

            var factor = _renderProperties.IsFacing(EODirection.Down, EODirection.Left) ? -1 : 1;
            var isDownOrRight = _renderProperties.IsFacing(EODirection.Down, EODirection.Right);

            resX += (parentCharacterDrawArea.Width / 1.5f - 3) * factor;
            if (_renderProperties.AttackFrame == 2)
                resX += 2 * factor;
            else if (_renderProperties.AttackFrame == 1 && _renderProperties.IsRangedWeapon)
                resX += (isDownOrRight ? 6 : 4) * factor;

            resY -= 1 + _renderProperties.Gender;
            if (_renderProperties.IsActing(CharacterActionState.Walking))
                resY -= 1;
            else if (_renderProperties.AttackFrame == 1 && _renderProperties.IsRangedWeapon)
                resY += isDownOrRight ? 1 : 0;

            return new Vector2(resX, resY);
        }
    }
}
