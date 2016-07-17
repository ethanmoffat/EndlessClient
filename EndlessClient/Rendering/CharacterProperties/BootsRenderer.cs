// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using EOLib;
using EOLib.Domain.Character;
using EOLib.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class BootsRenderer : ICharacterPropertyRenderer
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly ICharacterRenderProperties _renderProperties;
        private readonly Texture2D _bootsTexture;
        private readonly IDataFile<ItemRecord> _itemFile;

        public BootsRenderer(SpriteBatch spriteBatch,
                             ICharacterRenderProperties renderProperties,
                             Texture2D bootsTexture,
                             IDataFile<ItemRecord> itemFile)
        {
            _spriteBatch = spriteBatch;
            _renderProperties = renderProperties;
            _bootsTexture = bootsTexture;
            _itemFile = itemFile;
        }

        public void Render(Rectangle parentCharacterDrawArea)
        {
            var offsets = GetOffsets();
            var drawLoc = new Vector2(parentCharacterDrawArea.X - 2 + offsets.X, parentCharacterDrawArea.Y + 49 + offsets.Y);

            _spriteBatch.Draw(_bootsTexture, drawLoc, null, Color.White, 0.0f, Vector2.Zero, 1.0f,
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
            int bootsOffX = 0, bootsOffY = 0;

            if (weaponIsMelee && _renderProperties.AttackFrame == 2)
            {
                bootsOffX = _renderProperties.IsFacing(EODirection.Down, EODirection.Left) ? -6 : 6;
                if (_renderProperties.Gender == 1 && _renderProperties.IsFacing(EODirection.Up, EODirection.Left))
                    bootsOffY = -1;
            }
            else if (!weaponIsMelee && _renderProperties.AttackFrame == 1)
            {
                if (_renderProperties.IsFacing(EODirection.Down, EODirection.Right))
                {
                    bootsOffX = 6;
                    bootsOffY = 1;
                }
                else
                    bootsOffX = _renderProperties.Gender == 1 ? 7 : 3;

                if (_renderProperties.IsFacing(EODirection.Down, EODirection.Left))
                    bootsOffX *= -1;
            }

            return new Vector2(bootsOffX, bootsOffY);
        }
    }
}
