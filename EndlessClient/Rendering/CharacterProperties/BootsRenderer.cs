using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class BootsRenderer : BaseCharacterPropertyRenderer
    {
        private readonly ISpriteSheet _bootsSheet;

        public override bool CanRender => _bootsSheet.HasTexture && _renderProperties.BootsGraphic != 0;

        public BootsRenderer(CharacterRenderProperties renderProperties,
                             ISpriteSheet bootsSheet)
            : base(renderProperties)
        {
            _bootsSheet = bootsSheet;
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            var offsets = GetOffsets(parentCharacterDrawArea);
            var drawLoc = new Vector2(parentCharacterDrawArea.X + offsets.X,
                                      // Center the Y coordinate over the bottom half of the character sprite
                                      parentCharacterDrawArea.Y + offsets.Y);
            Render(spriteBatch, _bootsSheet, drawLoc);
        }

        private Vector2 GetOffsets(Rectangle parentCharacterDrawArea)
        {
            var resX = -(float)Math.Floor(Math.Abs((float)_bootsSheet.SourceRectangle.Width - parentCharacterDrawArea.Width) / 2);
            var resY = (int)Math.Floor(parentCharacterDrawArea.Height / 3f) * 2 - 1 - _renderProperties.Gender;

            if (_renderProperties.IsActing(CharacterActionState.Walking))
            {
                resY -= 1;

                if (_renderProperties.RenderWalkFrame == 2)
                    resY -= 1;
            }
            else if (_renderProperties.RenderAttackFrame == 2)
            {
                var isDownOrLeft = _renderProperties.IsFacing(EODirection.Down, EODirection.Left);
                var factor = isDownOrLeft ? -1 : 1;
                var extra = !isDownOrLeft ? 2*_renderProperties.Gender : 0;

                resX += 2 * factor;
                resY += 1 * factor - extra;
            }
            else if (_renderProperties.RenderAttackFrame == 1 && _renderProperties.IsRangedWeapon)
            {
                var isDownOrLeft = _renderProperties.IsFacing(EODirection.Down, EODirection.Left);
                var isDownOrRight = _renderProperties.IsFacing(EODirection.Down, EODirection.Right);
                var factor = isDownOrLeft ? -1 : 1;
                var offset = isDownOrRight ? 5 : 3;
                var extra = !isDownOrRight ? 4 * _renderProperties.Gender : 1 * _renderProperties.Gender;

                resX += factor * (offset + extra);
            }
            else if (_renderProperties.SitState != SitState.Standing)
            {
                resX -= 3;
                resY += _renderProperties.IsFacing(EODirection.Left, EODirection.Up) ? -3 : 2;

                if (_renderProperties.SitState == SitState.Floor)
                {
                    var factor = _renderProperties.IsFacing(EODirection.Left, EODirection.Down) ? -1 : 1;

                    if (_renderProperties.Gender == 0)
                    {
                        resX += (_renderProperties.IsFacing(EODirection.Right, EODirection.Down) ? 1 : -1) * factor;
                        resY += _renderProperties.IsFacing(EODirection.Right, EODirection.Down) ? 2 : 3;
                    }
                    else if (_renderProperties.Gender == 1)
                    {
                        resX += (_renderProperties.IsFacing(EODirection.Right, EODirection.Down) ? 0 : -1) * factor;
                        resY += 4;
                    }
                }
            }

            return new Vector2(resX, resY);
        }
    }
}
