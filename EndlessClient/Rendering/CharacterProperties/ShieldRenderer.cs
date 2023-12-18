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
    public class ShieldRenderer : BaseCharacterPropertyRenderer
    {
        private readonly ISpriteSheet _shieldSheet;
        private readonly bool _isShieldOnBack;

        public override bool CanRender => _shieldSheet.HasTexture && _renderProperties.ShieldGraphic != 0;

        // NOTE: The original client flips the arrows when facing left or up from the standing frame.
        //       I'm guessing this is a Vult bug. The offsets are wonky if this is enabled and I don't feel like figuring them out.
        //protected override bool ShouldFlip => _renderProperties.SitState == SitState.Floor
        //? _isShieldOnBack && _renderProperties.IsFacing(EODirection.Left, EODirection.Right)
        //: base.ShouldFlip;

        public ShieldRenderer(CharacterRenderProperties renderProperties,
                              ISpriteSheet shieldSheet,
                              bool isShieldOnBack)
            : base(renderProperties)
        {
            _shieldSheet = shieldSheet;
            _isShieldOnBack = isShieldOnBack;
        }

        public override void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea, WeaponMetadata weaponMetadata)
        {
            var offsets = GetOffsets(parentCharacterDrawArea, weaponMetadata.Ranged);
            var drawLoc = new Vector2(parentCharacterDrawArea.X + offsets.X, parentCharacterDrawArea.Y + offsets.Y);
            Render(spriteBatch, _shieldSheet, drawLoc);
        }

        private Vector2 GetOffsets(Rectangle parentCharacterDrawArea, bool ranged)
        {
            float resX, resY;

            if (_isShieldOnBack)
            {
                resX = -(float)Math.Floor(Math.Abs((float)_shieldSheet.SourceRectangle.Width - parentCharacterDrawArea.Width) / 2);
                resY = -(float)Math.Floor(parentCharacterDrawArea.Height / 3f) - _renderProperties.Gender;

                if (_renderProperties.RenderAttackFrame == 2)
                    resX += _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? 2 : -2;
                else if (ranged && _renderProperties.RenderAttackFrame == 1)
                {
                    // This currently does *not* match up perfectly with the original client. The original client doesn't keep
                    // the arrows aligned on the attack frame, so they look like they are sliding across the back of the character.
                    // I like it better this way (the offset is fixed for the arrows) so I'm not fixing it.
                    var factor = _renderProperties.IsFacing(EODirection.Down, EODirection.Left) ? -1 : 1;
                    var extra = _renderProperties.Gender * 2;
                    resX += factor * (1 + extra);
                }
                else if (_renderProperties.SitState != SitState.Standing)
                {
                    // These are the same offsets as hair

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
            }
            else
            {
                resX = (float)Math.Floor(Math.Abs((float)_shieldSheet.SourceRectangle.Width - parentCharacterDrawArea.Width) / 2);
                resY = (float)Math.Floor(Math.Abs((float)_shieldSheet.SourceRectangle.Height - parentCharacterDrawArea.Height) / 2) + 5;

                if (_renderProperties.IsFacing(EODirection.Down, EODirection.Left))
                {
                    resX -= parentCharacterDrawArea.Width * 1.5f;

                    if (_renderProperties.RenderAttackFrame == 2)
                        resX -= 2;

                    resX += 2;
                }
                else
                {
                    resX -= parentCharacterDrawArea.Width / 1.5f;

                    if (_renderProperties.RenderAttackFrame == 2)
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
