// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using AutomaticTypeMapper;
using EndlessClient.Rendering.Character;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.IO;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;

namespace EndlessClient.Rendering.CharacterProperties
{
    [MappedType(BaseType = typeof(ICharacterPropertyRendererBuilder))]
    public class CharacterPropertyRendererBuilder : ICharacterPropertyRendererBuilder
    {
        private readonly IEIFFileProvider _eifFileProvider;

        public CharacterPropertyRendererBuilder(IEIFFileProvider eifFileProvider)
        {
            _eifFileProvider = eifFileProvider;
        }

        public IEnumerable<ICharacterPropertyRenderer> BuildList(ICharacterTextures textures,
                                                                 ICharacterRenderProperties renderProperties)
        {
            bool shieldAdded = false, weaponAdded = false;

            if (IsShieldBehindCharacter(renderProperties))
            {
                shieldAdded = true;
                yield return new ShieldRenderer(renderProperties, textures.Shield);
            }

            if (IsWeaponBehindCharacter(renderProperties))
            {
                weaponAdded = true;
                yield return new WeaponRenderer(renderProperties, textures.Weapon, EIFFile);
            }

            yield return new SkinRenderer(renderProperties, textures.Skin);
            yield return new FaceRenderer(renderProperties, textures.Face, textures.Skin);
            yield return new EmoteRenderer(renderProperties, textures.Emote, textures.Skin);

            yield return new BootsRenderer(renderProperties, textures.Boots, EIFFile);
            yield return new ArmorRenderer(renderProperties, textures.Armor);
            if (!weaponAdded)
                yield return new WeaponRenderer(renderProperties, textures.Weapon, EIFFile);

            if (IsHairOnTopOfHat(renderProperties))
            {
                yield return new HatRenderer(renderProperties, textures.Hat, textures.Hair);
                yield return new HairRenderer(renderProperties, textures.Hair);
            }
            else
            {
                yield return new HairRenderer(renderProperties, textures.Hair);
                yield return new HatRenderer(renderProperties, textures.Hat, textures.Hair);
            }

            if (!shieldAdded)
                yield return new ShieldRenderer(renderProperties, textures.Shield);
        }

        private bool IsShieldBehindCharacter(ICharacterRenderProperties renderProperties)
        {
            return renderProperties.IsFacing(EODirection.Right, EODirection.Down) && IsShieldOnBack(renderProperties);
        }

        private bool IsWeaponBehindCharacter(ICharacterRenderProperties renderProperties)
        {
             var weaponInfo = EIFFile.Data.SingleOrDefault(
                x => x.Type == ItemType.Weapon &&
                     x.DollGraphic == renderProperties.WeaponGraphic);

            var pass1 = renderProperties.AttackFrame < 2;
            var pass2 = renderProperties.IsFacing(EODirection.Up, EODirection.Left);
            var pass3 = weaponInfo == null || weaponInfo.SubType == ItemSubType.Ranged;

            return pass1 || pass2 || pass3;
        }

        private bool IsHairOnTopOfHat(ICharacterRenderProperties renderProperties)
        {
            //todo: i might have this backwards...

            var hatInfo = EIFFile.Data.SingleOrDefault(
                x => x.Type == ItemType.Hat &&
                     x.DollGraphic == renderProperties.HatGraphic);

            return hatInfo != null && hatInfo.SubType == ItemSubType.FaceMask;
        }

        private bool IsShieldOnBack(ICharacterRenderProperties renderProperties)
        {
            if (EIFFile == null || !EIFFile.Data.Any())
                return false;

            var itemData = EIFFile.Data;
            var shieldInfo = itemData.SingleOrDefault(x => x.Type == ItemType.Shield &&
                                                           x.DollGraphic == renderProperties.ShieldGraphic);

            return shieldInfo != null &&
                   (shieldInfo.Name == "Bag" ||
                    shieldInfo.SubType == ItemSubType.Arrows ||
                    shieldInfo.SubType == ItemSubType.Wings);
        }

        private IPubFile<EIFRecord> EIFFile => _eifFileProvider.EIFFile ?? new EIFFile();
    }
}
