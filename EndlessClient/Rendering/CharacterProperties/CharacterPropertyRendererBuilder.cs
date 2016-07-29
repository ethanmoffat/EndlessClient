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
    public class CharacterPropertyRendererBuilder
    {
        private readonly ICharacterTextures _textures;
        private readonly IEIFFileProvider _eifFileProvider;

        public CharacterPropertyRendererBuilder(ICharacterTextures textures,
                                                IEIFFileProvider eifFileProvider)
        {
            _textures = textures;
            _eifFileProvider = eifFileProvider;
        }

        public IEnumerable<ICharacterPropertyRenderer> BuildList(ICharacterRenderProperties renderProperties)
        {
            var rendererList = new List<ICharacterPropertyRenderer>();

            if (ShieldEquipped(renderProperties) && IsShieldBehindCharacter(renderProperties))
                rendererList.Add(new ShieldRenderer(renderProperties, _textures.Shield));

            if (WeaponEquipped(renderProperties) && IsWeaponBehindCharacter(renderProperties))
                rendererList.Add(new WeaponRenderer(renderProperties, _textures.Weapon, EIFFile));

            rendererList.Add(new SkinRenderer(renderProperties, _textures.Skin, EIFFile));
            if (IsCharacterDoingEmote(renderProperties))
            {
                rendererList.Add(new FaceRenderer(renderProperties, _textures.Face, EIFFile));
                rendererList.Add(new EmoteRenderer(renderProperties, _textures.Emote, EIFFile));
            }

            if (BootsEquipped(renderProperties))
                rendererList.Add(new BootsRenderer(renderProperties, _textures.Boots, EIFFile));

            if (ArmorEquipped(renderProperties))
                rendererList.Add(new ArmorRenderer(renderProperties, _textures.Armor, EIFFile));

            if (WeaponEquipped(renderProperties) && !rendererList.OfType<WeaponRenderer>().Any())
                rendererList.Add(new WeaponRenderer(renderProperties, _textures.Weapon, EIFFile));

            var hairOnTopOfHat = new List<ICharacterPropertyRenderer>();
            if (HatEquipped(renderProperties))
                hairOnTopOfHat.Add(new HatRenderer(renderProperties, _textures.Hat, EIFFile));
            if (!IsBald(renderProperties))
                hairOnTopOfHat.Add(new HairRenderer(renderProperties, _textures.Hair, EIFFile));
            if (hairOnTopOfHat.Any())
                rendererList.AddRange(IsHairOnTopOfHat(renderProperties) ? hairOnTopOfHat : hairOnTopOfHat.ToArray().Reverse());

            if (ShieldEquipped(renderProperties) && !rendererList.OfType<ShieldRenderer>().Any())
                rendererList.Add(new ShieldRenderer(renderProperties, _textures.Shield));

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

        private bool IsCharacterDoingEmote(ICharacterRenderProperties renderProperties)
        {
            return renderProperties.CurrentAction == CharacterActionState.Emote &&
                   renderProperties.EmoteFrame > 0;
        }

        private bool IsHairOnTopOfHat(ICharacterRenderProperties renderProperties)
        {
            //todo: i might have this backwards...

            var hatInfo = EIFFile.Data.SingleOrDefault(
                x => x.Type == ItemType.Hat &&
                     x.DollGraphic == renderProperties.HatGraphic);

            return hatInfo != null && hatInfo.SubType == ItemSubType.FaceMask;
        }

        private bool BootsEquipped(ICharacterRenderProperties renderProperties)
        {
            return renderProperties.BootsGraphic != 0 && _textures.Boots != null;
        }

        private bool ArmorEquipped(ICharacterRenderProperties renderProperties)
        {
            return renderProperties.ArmorGraphic != 0 && _textures.Armor != null;
        }

        private bool HatEquipped(ICharacterRenderProperties renderProperties)
        {
            return renderProperties.HatGraphic != 0 && _textures.Hat != null;
        }

        private bool ShieldEquipped(ICharacterRenderProperties renderProperties)
        {

            return renderProperties.ShieldGraphic != 0 && _textures.Shield != null;
        }

        private bool WeaponEquipped(ICharacterRenderProperties renderProperties)
        {
            return renderProperties.WeaponGraphic != 0 && _textures.Weapon != null;
        }

        private bool IsBald(ICharacterRenderProperties renderProperties)
        {
            return _textures.Hair == null || renderProperties.HairStyle == 0;
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
