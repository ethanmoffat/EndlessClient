// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using EOLib;
using EOLib.Domain.Character;
using EOLib.IO;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class HairRenderLocationCalculator
    {
        private readonly IDataFile<ItemRecord> _itemFile;
        private readonly ICharacterRenderProperties _renderProperties;

        public HairRenderLocationCalculator(IDataFile<ItemRecord> itemFile,
                                            ICharacterRenderProperties renderProperties)
        {
            _itemFile = itemFile;
            _renderProperties = renderProperties;
        }

        public Vector2 CalculateDrawLocationOfCharacterHair(Rectangle parentCharacterDrawArea)
        {
            var offsets = GetOffsets();
            var flippedOffset = _renderProperties.IsFacing(EODirection.Up, EODirection.Right) ? 2 : 0;

            return new Vector2(parentCharacterDrawArea.X + offsets.X + flippedOffset,
                               parentCharacterDrawArea.Y + offsets.Y);
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
            int hatOffX = 0, hatOffY = 0;

            if (weaponIsMelee && _renderProperties.AttackFrame == 2)
            {
                hatOffX = _renderProperties.Gender == 1 ? 6 : 8;
                if (_renderProperties.IsFacing(EODirection.Down, EODirection.Left))
                    hatOffX *= -1;

                if (_renderProperties.IsFacing(EODirection.Down, EODirection.Right))
                    hatOffY = _renderProperties.Gender == 1 ? 5 : 6;
            }
            else if (!weaponIsMelee && _renderProperties.AttackFrame == 1)
            {
                hatOffX = _renderProperties.Gender == 1 ? 3 : 1;
                if (_renderProperties.IsFacing(EODirection.Down, EODirection.Left))
                    hatOffX *= -1;

                if (_renderProperties.IsFacing(EODirection.Down, EODirection.Right))
                    hatOffY = _renderProperties.Gender == 1 ? 1 : 0;
            }

            return new Vector2(hatOffX, hatOffY);
        }
    }
}
