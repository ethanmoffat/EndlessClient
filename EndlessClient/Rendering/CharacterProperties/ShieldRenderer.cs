// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Sprites;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class ShieldRenderer : ICharacterPropertyRenderer
    {
        private readonly ICharacterRenderProperties _renderProperties;
        private readonly ISpriteSheet _shieldSheet;
        private readonly bool _isShieldOnBack;

        public bool CanRender => _shieldSheet.HasTexture && _renderProperties.ShieldGraphic != 0;

        public ShieldRenderer(ICharacterRenderProperties renderProperties,
                              ISpriteSheet shieldSheet,
                              bool isShieldOnBack)
        {
            _renderProperties = renderProperties;
            _shieldSheet = shieldSheet;
            _isShieldOnBack = isShieldOnBack;
        }

        public void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            var offsets = GetOffsets(parentCharacterDrawArea);
            var drawLoc = new Vector2(parentCharacterDrawArea.X + offsets.X, parentCharacterDrawArea.Y + offsets.Y);

            spriteBatch.Draw(_shieldSheet.SheetTexture, drawLoc, _shieldSheet.SourceRectangle, Color.White, 0.0f, Vector2.Zero, 1.0f,
                             _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                             0.0f);
        }

        private Vector2 GetOffsets(Rectangle parentCharacterDrawArea)
        {
            var resX = 0f;
            var resY = 0f;

            if (_isShieldOnBack)
            {
                resX = -(float)Math.Floor(Math.Abs((float)_shieldSheet.SourceRectangle.Width - parentCharacterDrawArea.Width) / 2);
                resY = -(float)Math.Floor(parentCharacterDrawArea.Height / 3f) - 1;

                if (_renderProperties.AttackFrame == 2)
                    resX -= 2;
            }
            else
            {
                resX = (float)Math.Floor(Math.Abs((float)_shieldSheet.SourceRectangle.Width - parentCharacterDrawArea.Width) / 2);
                resY = (float)Math.Floor(Math.Abs((float)_shieldSheet.SourceRectangle.Height - parentCharacterDrawArea.Height) / 2) + 5;

                if (_renderProperties.IsFacing(EODirection.Down, EODirection.Left))
                {
                    resX -= parentCharacterDrawArea.Width * 1.5f;

                    if (_renderProperties.AttackFrame == 2)
                        resX -= 2;

                    resX += 2;
                }
                else
                {
                    resX -= parentCharacterDrawArea.Width / 1.5f;

                    if (_renderProperties.AttackFrame == 2)
                        resX += 2;

                    resX -= 3;
                }

                resY -= _renderProperties.Gender;
                if (_renderProperties.IsActing(CharacterActionState.Walking))
                    resY -= 1;
            }

            return new Vector2(resX, resY);
        }
    }
}
