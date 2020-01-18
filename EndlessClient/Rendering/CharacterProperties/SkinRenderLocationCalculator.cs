// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
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
            var resX = -(float)Math.Floor(Math.Abs((float)skinRectangle.Width - parentCharacterDrawArea.Width) / 2);
            var resY = -(float)Math.Floor(Math.Abs((float)skinRectangle.Height - parentCharacterDrawArea.Height) / 2);

            if (_renderProperties.IsActing(CharacterActionState.SpellCast))
                resY -= 2;

            // This specific frame is a bitch
            if (_renderProperties.Gender == 1 && _renderProperties.AttackFrame == 1)
                resX += _renderProperties.IsFacing(EOLib.EODirection.Up, EOLib.EODirection.Right) ? 2 : -2;

            return new Vector2(parentCharacterDrawArea.X + resX, parentCharacterDrawArea.Y + resY);
        }
    }
}
