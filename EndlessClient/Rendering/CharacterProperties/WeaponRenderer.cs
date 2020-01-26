// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class WeaponRenderer : ICharacterPropertyRenderer
    {
        private readonly ICharacterRenderProperties _renderProperties;
        private readonly ISpriteSheet _weaponSheet;
        private readonly bool _isRangedWeapon;

        public bool CanRender => _weaponSheet.HasTexture && _renderProperties.WeaponGraphic != 0;

        public WeaponRenderer(ICharacterRenderProperties renderProperties,
                              ISpriteSheet weaponSheet,
                              bool isRangedWeapon)
        {
            _renderProperties = renderProperties;
            _weaponSheet = weaponSheet;
            _isRangedWeapon = isRangedWeapon;
        }

        public void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            if (_renderProperties.IsActing(CharacterActionState.Sitting, CharacterActionState.SpellCast))
                return;

            var offsets = GetOffsets(parentCharacterDrawArea);
            var drawLoc = new Vector2(parentCharacterDrawArea.X + offsets.X, parentCharacterDrawArea.Y + offsets.Y);

            spriteBatch.Draw(_weaponSheet.SheetTexture, drawLoc, _weaponSheet.SourceRectangle, Color.White, 0.0f, Vector2.Zero, 1.0f,
                             _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                             0.0f);
        }

        private Vector2 GetOffsets(Rectangle parentCharacterDrawArea)
        {
            float resX, resY;

            if (_isRangedWeapon)
            {
                resX = 0;
                resY = 0;
                //resX = -(float)Math.Floor(Math.Abs((float)_shieldSheet.SourceRectangle.Width - parentCharacterDrawArea.Width) / 2);
                //resY = -(float)Math.Floor(parentCharacterDrawArea.Height / 3f) - _renderProperties.Gender;

                //if (_renderProperties.AttackFrame == 2)
                //    resX += _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? 2 : -2;
            }
            else
            {
                resX = -(float)Math.Floor(Math.Abs((float)_weaponSheet.SourceRectangle.Width - parentCharacterDrawArea.Width) / 2);
                resY = -(float)Math.Floor(Math.Abs((float)_weaponSheet.SourceRectangle.Height - parentCharacterDrawArea.Height) / 2) - 5;

                if (_renderProperties.IsFacing(EODirection.Down, EODirection.Left))
                {
                    resX -= parentCharacterDrawArea.Width / 1.5f;

                    if (_renderProperties.AttackFrame == 2)
                        resX -= 2;

                    resX += 3;
                }
                else
                {
                    resX += parentCharacterDrawArea.Width / 1.5f;

                    if (_renderProperties.AttackFrame == 2)
                        resX += 2;

                    resX -= 3;
                }

                resY -= 1 + _renderProperties.Gender;
                if (_renderProperties.IsActing(CharacterActionState.Walking))
                    resY -= 1;
            }

            return new Vector2(resX, resY);
        }
    }
}
