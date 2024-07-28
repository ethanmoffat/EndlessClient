using EndlessClient.Rendering.Metadata.Models;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class ArmorRenderer : BaseCharacterPropertyRenderer
    {
        private readonly ISpriteSheet _armorSheet;

        public override bool CanRender => _armorSheet.HasTexture && _renderProperties.ArmorGraphic != 0;

        public ArmorRenderer(CharacterRenderProperties renderProperties,
                             ISpriteSheet armorSheet)
            : base(renderProperties)
        {
            _armorSheet = armorSheet;
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea, WeaponMetadata weaponMetadata)
        {
            var offsets = GetOffsets(parentCharacterDrawArea.Size.ToVector2(), weaponMetadata.Ranged);
            var drawLoc = new Vector2(parentCharacterDrawArea.X - 2 + offsets.X, parentCharacterDrawArea.Y + offsets.Y);
            Render(spriteBatch, _armorSheet, drawLoc);
        }

        private Vector2 GetOffsets(Vector2 parentCharacterSize, bool ranged)
        {
            var resX = -(float)Math.Floor(Math.Abs(_armorSheet.SourceRectangle.Width - parentCharacterSize.X) / 2);
            var resY = -(float)Math.Floor(Math.Abs(_armorSheet.SourceRectangle.Height - parentCharacterSize.Y) / 2);

            if ((_renderProperties.RenderAttackFrame == 2 && !ranged) ||
                (_renderProperties.RenderAttackFrame == 1 && ranged))
            {
                resX += _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? 4 : 0;

                if (ranged)
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
                if (_renderProperties.SitState == SitState.Chair)
                {
                    resX -= 1;
                    resY += _renderProperties.IsFacing(EODirection.Left, EODirection.Up) ? 2 : 0;
                }
                else
                {
                    resX += _renderProperties.IsFacing(EODirection.Left) ? _renderProperties.Gender : _renderProperties.IsFacing(EODirection.Up) ? -_renderProperties.Gender : 0;
                    resX -= _renderProperties.IsFacing(EODirection.Down, EODirection.Up) ? (2 - _renderProperties.Gender) : _renderProperties.Gender;
                    resY += _renderProperties.IsFacing(EODirection.Left, EODirection.Up) ? 12 : 9;
                }
            }
            else
            {
                resX += 2;
            }

            if (_renderProperties.SitState == SitState.Standing)
                resY -= (_renderProperties.IsActing(CharacterActionState.Walking) ? 4 : 3) + _renderProperties.Gender;

            return new Vector2(resX, resY);
        }
    }
}