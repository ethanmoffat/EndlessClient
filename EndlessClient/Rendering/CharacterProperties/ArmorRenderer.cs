// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.IO;
using EOLib.IO.Pub;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class ArmorRenderer : ICharacterPropertyRenderer
    {
        private readonly ICharacterRenderProperties _renderProperties;
        private readonly Texture2D _armorTexture;
        private readonly IPubFile<EIFRecord> _itemFile;

        public bool CanRender { get { return _armorTexture != null && _renderProperties.ArmorGraphic != 0; } }

        public ArmorRenderer(ICharacterRenderProperties renderProperties,
                             Texture2D armorTexture,
                             IPubFile<EIFRecord> itemFile)
        {
            _renderProperties = renderProperties;
            _armorTexture = armorTexture;
            _itemFile = itemFile;
        }

        public void Render(SpriteBatch spriteBatch, Rectangle parentCharacterDrawArea)
        {
            var offsets = GetOffsets();
            var drawLoc = new Vector2(parentCharacterDrawArea.X - 2 + offsets.X, parentCharacterDrawArea.Y + offsets.Y);

            spriteBatch.Draw(_armorTexture, drawLoc, null, Color.White, 0.0f, Vector2.Zero, 1.0f,
                             _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? SpriteEffects.FlipHorizontally : SpriteEffects.None,
                             0.0f);
        }

        private bool IsWeaponAMeleeWeapon()
        {
            var weaponInfo = _itemFile.Data.SingleOrDefault(
                x => x.Type == ItemType.Weapon &&
                     x.DollGraphic == _renderProperties.WeaponGraphic);

            return weaponInfo == null || weaponInfo.SubType != ItemSubType.Ranged;
        }

        private Vector2 GetOffsets()
        {
            var weaponIsMelee = IsWeaponAMeleeWeapon();
            var armorOffX = 0;
            var armorOffY = _renderProperties.CurrentAction == CharacterActionState.Walking ? -1 : 0;

            if (weaponIsMelee && _renderProperties.AttackFrame == 2)
            {
                armorOffX = _renderProperties.Gender == 1 ? 6 : 7;
                if (_renderProperties.IsFacing(EODirection.Down, EODirection.Left))
                    armorOffX *= -1;

                armorOffY += _renderProperties.IsFacing(EODirection.Down, EODirection.Right) ? 1 : -1;
            }
            else if (!weaponIsMelee && _renderProperties.AttackFrame == 1)
            {
                if (_renderProperties.IsFacing(EODirection.Down, EODirection.Right))
                {
                    armorOffX = 6;
                    armorOffY += 1;
                }
                else
                {
                    armorOffX = 4;
                }

                if (_renderProperties.IsFacing(EODirection.Down, EODirection.Left))
                    armorOffX *= -1;
            }

            return new Vector2(armorOffX, armorOffY);
        }
    }
}
