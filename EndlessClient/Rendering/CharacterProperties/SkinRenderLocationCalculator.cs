// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class SkinRenderLocationCalculator
    {
        private readonly ICharacterRenderProperties _renderProperties;

        public SkinRenderLocationCalculator(ICharacterRenderProperties renderProperties)
        {
            _renderProperties = renderProperties;
        }

        public Vector2 CalculateDrawLocationOfCharacterSkin(Rectangle skinRectangle, Rectangle parentCharacterDrawArea)
        {
            float resX, resY;

            // Ranged weapon attack frame is offset based on the Left or Right border of the parent character draw area
            // Ranged weapon graphics do not use the centering approach
            if (_renderProperties.IsRangedWeapon && _renderProperties.AttackFrame == 1)
            {
                var isFlipped = _renderProperties.IsFacing(EODirection.Right, EODirection.Up);
                var needsExtraOffset = _renderProperties.IsFacing(EODirection.Right, EODirection.Down);

                var startPixelX = isFlipped ? parentCharacterDrawArea.Left : parentCharacterDrawArea.Right;
                var offsetFactor = isFlipped ? 0 : -1;

                resX = startPixelX + ((needsExtraOffset ? 2 : 0) + skinRectangle.Width) * offsetFactor;

                // essentially, is it facing EODirection.Right?
                if (isFlipped && needsExtraOffset)
                    resX += 2;

                // male needs an extra +/- 2
                if (_renderProperties.Gender == 1)
                    resX += 2 * (isFlipped ? 1 : -1);

                resY = parentCharacterDrawArea.Y + (needsExtraOffset ? 1 : 0);
            }
            else
            {
                resX = -(float)Math.Floor(Math.Abs((float)skinRectangle.Width - parentCharacterDrawArea.Width) / 2);
                resY = -(float)Math.Floor(Math.Abs((float)skinRectangle.Height - parentCharacterDrawArea.Height) / 2);

                if (_renderProperties.IsActing(CharacterActionState.SpellCast))
                    resY -= 2;

                // This specific frame is a bitch
                if (_renderProperties.Gender == 1 && _renderProperties.AttackFrame == 1)
                    resX += _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? 2 : -2;

                resX += parentCharacterDrawArea.X;
                resY += parentCharacterDrawArea.Y;
            }

            return new Vector2(resX, resY);
        }
    }
}
