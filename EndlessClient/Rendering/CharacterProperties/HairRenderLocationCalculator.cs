using System;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class HairRenderLocationCalculator
    {
        private readonly CharacterRenderProperties _renderProperties;

        public HairRenderLocationCalculator(CharacterRenderProperties renderProperties)
        {
            _renderProperties = renderProperties;
        }

        public Vector2 CalculateDrawLocationOfCharacterHair(Rectangle hairRectangle, Rectangle parentCharacterDrawArea)
        {
            var resX = -(float)Math.Floor(Math.Abs((float)hairRectangle.Width - parentCharacterDrawArea.Width) / 2) - 1;
            var resY = -(float)Math.Floor(Math.Abs(hairRectangle.Height - (parentCharacterDrawArea.Height / 2f)) / 2) - _renderProperties.Gender;

            var isFlipped = _renderProperties.IsFacing(EODirection.Up, EODirection.Right);

            if (_renderProperties.IsRangedWeapon && _renderProperties.RenderAttackFrame == 1)
            {
                var rangedXOff = _renderProperties.Gender == 0 ? 1 : 3;
                resX += rangedXOff * (isFlipped ? 1 : -1);
                resY += _renderProperties.IsFacing(EODirection.Down, EODirection.Right) ? _renderProperties.Gender : 0;
            }
            else if (_renderProperties.RenderAttackFrame == 2)
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
            else if (_renderProperties.SitState != SitState.Standing)
            {
                resX -= 3;

                var flootSitFactor = _renderProperties.SitState == SitState.Floor ? 2 : 1;
                if (_renderProperties.IsFacing(EODirection.Right, EODirection.Down))
                {
                    resY += (9 + _renderProperties.Gender) * flootSitFactor;
                }
                else
                {
                    if (_renderProperties.SitState == SitState.Floor)
                    {
                        resX += _renderProperties.IsFacing(EODirection.Left) ? 2 : -2;
                        resY -= 1;
                    }

                    resY += (11 + _renderProperties.Gender) * flootSitFactor;
                }
            }

            var flippedOffset = isFlipped ? 2 : 0;
            return parentCharacterDrawArea.Location.ToVector2() + new Vector2(resX + flippedOffset, resY);
        }
    }
}
