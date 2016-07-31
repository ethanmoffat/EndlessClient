// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.IO;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class CharacterPropertyRendererBuilder : ICharacterPropertyRendererBuilder
    {
        private readonly IEIFFileProvider _eifFileProvider;

        public CharacterPropertyRendererBuilder(IEIFFileProvider eifFileProvider)
        {
            _eifFileProvider = eifFileProvider;
        }

        public List<ICharacterPropertyRenderer> BuildList(ICharacterTextures textures,
                                                          ICharacterRenderProperties renderProperties)
        {
            var rendererList = new List<ICharacterPropertyRenderer>();

            if (IsShieldBehindCharacter(renderProperties))
                rendererList.Add(new ShieldRenderer(renderProperties, textures.Shield));

            if (IsWeaponBehindCharacter(renderProperties))
                rendererList.Add(new WeaponRenderer(renderProperties, textures.Weapon, EIFFile));

            rendererList.Add(new SkinRenderer(renderProperties, textures.Skin, EIFFile));
            rendererList.Add(new FaceRenderer(renderProperties, textures.Face, EIFFile));
            rendererList.Add(new EmoteRenderer(renderProperties, textures.Emote, EIFFile));

            rendererList.Add(new BootsRenderer(renderProperties, textures.Boots, EIFFile));
            rendererList.Add(new ArmorRenderer(renderProperties, textures.Armor, EIFFile));
            if (!rendererList.OfType<WeaponRenderer>().Any())
                rendererList.Add(new WeaponRenderer(renderProperties, textures.Weapon, EIFFile));

            if (IsHairOnTopOfHat(renderProperties))
            {
                rendererList.Add(new HatRenderer(renderProperties, textures.Hat, EIFFile));
                rendererList.Add(new HairRenderer(renderProperties, textures.Hair, EIFFile));
            }
            else
            {
                rendererList.Add(new HairRenderer(renderProperties, textures.Hair, EIFFile));
                rendererList.Add(new HatRenderer(renderProperties, textures.Hat, EIFFile));
            }

            if (!rendererList.OfType<ShieldRenderer>().Any())
                rendererList.Add(new ShieldRenderer(renderProperties, textures.Shield));

            return rendererList;
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

        private IPubFile<EIFRecord> EIFFile
        {
            get { return _eifFileProvider.EIFFile ?? new EIFFile(); }
        }
    }
}
