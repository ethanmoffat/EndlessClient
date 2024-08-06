using System;
using EndlessClient.Rendering.Metadata.Models;
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

        public WeaponRenderer(CharacterRenderProperties renderProperties,
                              ISpriteSheet weaponSheet)
            : base(renderProperties)
        {
            _weaponSheet = weaponSheet;
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea, WeaponMetadata weaponMetadata)
        {
            if (_renderProperties.IsActing(CharacterActionState.Sitting, CharacterActionState.SpellCast) ||
                (_renderProperties.CurrentAction == CharacterActionState.Emote && _renderProperties.SitState != SitState.Standing))
                return;

            var offsets = GetOffsets(parentCharacterDrawArea, weaponMetadata.Ranged);
            var drawLoc = new Vector2(parentCharacterDrawArea.X + offsets.X, parentCharacterDrawArea.Y + offsets.Y);
            Render(spriteBatch, _weaponSheet, drawLoc);
        }

        private Vector2 GetOffsets(Rectangle parentCharacterDrawArea, bool ranged)
        {
            float resX, resY;

            resX = -(float)Math.Floor(Math.Abs((float)_weaponSheet.SourceRectangle.Width - parentCharacterDrawArea.Width) / 2);
            resY = -(float)Math.Floor(Math.Abs((float)_weaponSheet.SourceRectangle.Height - parentCharacterDrawArea.Height) / 2) - 5;

            var factor = _renderProperties.IsFacing(EODirection.Down, EODirection.Left) ? -1 : 1;
            var isDownOrRight = _renderProperties.IsFacing(EODirection.Down, EODirection.Right);

            resX += (parentCharacterDrawArea.Width / 1.5f - 3) * factor;
            if (_renderProperties.RenderAttackFrame == 2)
                resX += 2 * factor;
            else if (_renderProperties.RenderAttackFrame == 1 && ranged)
                resX += (isDownOrRight ? 6 : 4) * factor;

            resY -= 1 + _renderProperties.Gender;
            if (_renderProperties.IsActing(CharacterActionState.Walking))
                resY -= 1;
            else if (_renderProperties.RenderAttackFrame == 1 && ranged)
                resY += isDownOrRight ? 1 : 0;

            return new Vector2(resX, resY);
        }
    }
}
