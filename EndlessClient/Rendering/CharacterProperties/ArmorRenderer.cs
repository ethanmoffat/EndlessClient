using System;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class ArmorRenderer : BaseCharacterPropertyRenderer
    {
        private readonly ISpriteSheet _armorSheet;

        public override bool CanRender => _armorSheet.HasTexture && _renderProperties.ArmorGraphic != 0;

        public ArmorRenderer(ICharacterRenderProperties renderProperties,
                             ISpriteSheet armorSheet)
            : base(renderProperties)
        {
            _armorSheet = armorSheet;
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            var offsets = GetOffsets(parentCharacterDrawArea.Size.ToVector2());
            var drawLoc = new Vector2(parentCharacterDrawArea.X - 2 + offsets.X, parentCharacterDrawArea.Y + offsets.Y);
            Render(spriteBatch, _armorSheet, drawLoc);
        }

        private Vector2 GetOffsets(Vector2 parentCharacterSize)
        {
            var resX = -(float)Math.Floor(Math.Abs(_armorSheet.SourceRectangle.Width - parentCharacterSize.X) / 2);
            var resY = -(float)Math.Floor(Math.Abs(_armorSheet.SourceRectangle.Height - parentCharacterSize.Y) / 2);

            if ((_renderProperties.AttackFrame == 2 && !_renderProperties.IsRangedWeapon) ||
                (_renderProperties.AttackFrame == 1 && _renderProperties.IsRangedWeapon))
            {
                resX += _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? 4 : 0;

                if (_renderProperties.IsRangedWeapon)
                {
                    var factor = _renderProperties.IsFacing(EODirection.Down, EODirection.Left) ? -1 : 1;
                    resX += _renderProperties.IsFacing(EODirection.Left, EODirection.Up)
                        ? 2 * factor
                        : 4 * factor;

                    resY += _renderProperties.IsFacing(EODirection.Down, EODirection.Right) ? 1 : 0;
                }
            }
            else if (_renderProperties.SitState != SitState.Standing)
            {
                // todo: floor sitting offsets
                resX -= 1;
            }
            else
            {
                resX += 2;
            }

            // todo: floor sitting offsets
            if (_renderProperties.SitState == SitState.Standing)
                resY -= (_renderProperties.IsActing(CharacterActionState.Walking) ? 4 : 3) + _renderProperties.Gender;
            else
                resY += _renderProperties.IsFacing(EODirection.Left, EODirection.Up) ? 2 : 0;

            return new Vector2(resX, resY);
        }
    }
}
