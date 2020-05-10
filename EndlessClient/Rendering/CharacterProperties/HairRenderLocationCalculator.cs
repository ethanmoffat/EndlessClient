using System;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class HairRenderLocationCalculator
    {
        private readonly ICharacterRenderProperties _renderProperties;

        public HairRenderLocationCalculator(ICharacterRenderProperties renderProperties)
        {
            _renderProperties = renderProperties;
        }

        public Vector2 CalculateDrawLocationOfCharacterHair(Rectangle hairRectangle, Rectangle parentCharacterDrawArea)
        {
            var resX = -(float)Math.Floor(Math.Abs((float)hairRectangle.Width - parentCharacterDrawArea.Width) / 2) - 1;
            var resY = -(float)Math.Floor(Math.Abs(hairRectangle.Height - (parentCharacterDrawArea.Height / 2f)) / 2) - _renderProperties.Gender;

            var isFlipped = _renderProperties.IsFacing(EODirection.Up, EODirection.Right);

            if (_renderProperties.IsRangedWeapon && _renderProperties.AttackFrame == 1)
            {
                var rangedXOff = _renderProperties.Gender == 0 ? 1 : 3;
                resX += rangedXOff * (isFlipped ? 1 : -1);
                resY += _renderProperties.IsFacing(EODirection.Down, EODirection.Right) ? _renderProperties.Gender : 0;
            }
            else if (_renderProperties.AttackFrame == 2)
            {
                resX += isFlipped ? 4 : -4;
                resX += _renderProperties.IsFacing(EODirection.Up)
                    ? _renderProperties.Gender * -2
                    : _renderProperties.IsFacing(EODirection.Left)
                        ? _renderProperties.Gender * 2
                        : 0;

                resY += _renderProperties.IsFacing(EODirection.Up, EODirection.Left) ? 1 : 5;
                resY -= _renderProperties.IsFacing(EODirection.Right, EODirection.Down) ? _renderProperties.Gender : 0;
            }

            var flippedOffset = isFlipped ? 2 : 0;
            return parentCharacterDrawArea.Location.ToVector2() + new Vector2(resX + flippedOffset, resY);
        }
    }
}
