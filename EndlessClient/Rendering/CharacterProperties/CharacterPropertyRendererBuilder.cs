// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EndlessClient.Content;
using EndlessClient.Rendering.Character;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.IO;
using EOLib.IO.Extensions;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;

namespace EndlessClient.Rendering.CharacterProperties
{
    [MappedType(BaseType = typeof(ICharacterPropertyRendererBuilder))]
    public class CharacterPropertyRendererBuilder : ICharacterPropertyRendererBuilder
    {
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly IHatConfigurationProvider _hatConfigurationProvider;

        public CharacterPropertyRendererBuilder(IEIFFileProvider eifFileProvider,
                                                IHatConfigurationProvider hatConfigurationProvider)
        {
            _eifFileProvider = eifFileProvider;
            _hatConfigurationProvider = hatConfigurationProvider;
        }

        public IEnumerable<ICharacterPropertyRenderer> BuildList(ICharacterTextures textures,
                                                                 ICharacterRenderProperties renderProperties)
        {
            bool shieldAdded = false, weaponAdded = false;

            // Melee weapons render extra behind the character
            yield return new WeaponRenderer(renderProperties, textures.WeaponExtra, EIFFile.IsRangedWeapon(renderProperties.WeaponGraphic));

            if (IsShieldBehindCharacter(renderProperties))
            {
                shieldAdded = true;
                yield return new ShieldRenderer(renderProperties, textures.Shield, EIFFile.IsShieldOnBack(renderProperties.ShieldGraphic));
            }

            if (IsWeaponBehindCharacter(renderProperties))
            {
                weaponAdded = true;
                yield return new WeaponRenderer(renderProperties, textures.Weapon, EIFFile.IsRangedWeapon(renderProperties.WeaponGraphic));
            }

            yield return new SkinRenderer(renderProperties, textures.Skin);
            yield return new FaceRenderer(renderProperties, textures.Face, textures.Skin);
            yield return new EmoteRenderer(renderProperties, textures.Emote, textures.Skin);

            yield return new BootsRenderer(renderProperties, textures.Boots);
            yield return new ArmorRenderer(renderProperties, textures.Armor);
            if (!weaponAdded)
                yield return new WeaponRenderer(renderProperties, textures.Weapon, EIFFile.IsRangedWeapon(renderProperties.WeaponGraphic));

            var hatMaskType = GetHatMaskType(renderProperties);

            if (hatMaskType == HatMaskType.FaceMask)
            {
                yield return new HatRenderer(renderProperties, textures.Hat, textures.Hair);
                yield return new HairRenderer(renderProperties, textures.Hair);
            }
            else
            {
                if (hatMaskType == HatMaskType.Standard)
                    yield return new HairRenderer(renderProperties, textures.Hair);

                yield return new HatRenderer(renderProperties, textures.Hat, textures.Hair);
            }

            if (!shieldAdded)
                yield return new ShieldRenderer(renderProperties, textures.Shield, EIFFile.IsShieldOnBack(renderProperties.ShieldGraphic));
        }

        private bool IsShieldBehindCharacter(ICharacterRenderProperties renderProperties)
        {
            return renderProperties.IsFacing(EODirection.Right, EODirection.Down) && EIFFile.IsShieldOnBack(renderProperties.ShieldGraphic);
        }

        private bool IsWeaponBehindCharacter(ICharacterRenderProperties renderProperties)
        {
             var weaponInfo = EIFFile.Data.FirstOrDefault(
                x => x.Type == ItemType.Weapon &&
                     x.DollGraphic == renderProperties.WeaponGraphic);

            var pass1 = renderProperties.AttackFrame < 2;
            var pass2 = renderProperties.IsFacing(EODirection.Up, EODirection.Left);
            var pass3 = weaponInfo == null || weaponInfo.SubType == ItemSubType.Ranged;

            return pass1 || pass2 || pass3;
        }

        private HatMaskType GetHatMaskType(ICharacterRenderProperties renderProperties)
        {
            //todo: i might have this backwards...

            var hatInfo = EIFFile.Data.FirstOrDefault(
                x => x.Type == ItemType.Hat &&
                     x.DollGraphic == renderProperties.HatGraphic);

            _hatConfigurationProvider.HatMasks.TryGetValue(hatInfo?.ID ?? 0, out var hatMaskType);
            return hatMaskType;
        }

        private IPubFile<EIFRecord> EIFFile => _eifFileProvider.EIFFile ?? new EIFFile();
    }
}
