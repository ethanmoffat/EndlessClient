// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Linq;
using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
using EOLib.IO;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering
{
    public class CharacterTextures : ICharacterTextures
    {
        private readonly IEIFFileProvider _eifFileProvider;
        private readonly ICharacterSpriteCalculator _calc;
        private readonly ICharacterRenderProperties _characterRenderProperties;

        public Texture2D Boots { get; private set; }
        public Texture2D Armor { get; private set; }
        public Texture2D Hat { get; private set; }
        public Texture2D Shield { get; private set; }
        public Texture2D Weapon { get; private set; }

        public Texture2D Hair { get; private set; }
        public ISpriteSheet Skin { get; private set; }

        public ISpriteSheet Emote { get; private set; }
        public ISpriteSheet Face { get; private set; }

        public CharacterTextures(IEIFFileProvider eifFileProvider,
                                 ICharacterSpriteCalculator calc,
                                 ICharacterRenderProperties characterRenderProperties)
        {
            _eifFileProvider = eifFileProvider;
            _calc = calc;
            _characterRenderProperties = characterRenderProperties;
        }

        public void Refresh()
        {
            Boots = _calc.GetBootsTexture(BowIsEquipped).SheetTexture;
            Armor = _calc.GetArmorTexture(BowIsEquipped).SheetTexture;
            Hat = _calc.GetHatTexture().SheetTexture;
            Shield = _calc.GetShieldTexture(ShieldIsOnBack).SheetTexture;
            Weapon = _calc.GetWeaponTexture(BowIsEquipped).SheetTexture;

            Hair = _calc.GetHairTexture().SheetTexture;
            Skin = _calc.GetSkinTexture(BowIsEquipped);
            Emote = _calc.GetEmoteTexture();
            Face = _calc.GetFaceTexture();
        }


        private bool BowIsEquipped
        {
            get
            {
                if (EIFFile == null || EIFFile.Data == null)
                    return false;

                var itemData = EIFFile.Data;
                var weaponInfo = itemData.SingleOrDefault(x => x.Type == ItemType.Weapon &&
                                                               x.DollGraphic == _characterRenderProperties.WeaponGraphic);

                return weaponInfo != null && weaponInfo.SubType == ItemSubType.Ranged;
            }
        }

        private bool ShieldIsOnBack
        {
            get
            {
                if (EIFFile == null || EIFFile.Data == null)
                    return false;

                var itemData = EIFFile.Data;
                var shieldInfo = itemData.SingleOrDefault(x => x.Type == ItemType.Shield &&
                                                               x.DollGraphic == _characterRenderProperties.ShieldGraphic);

                return shieldInfo != null &&
                       (shieldInfo.Name == "Bag" ||
                        shieldInfo.SubType == ItemSubType.Arrows ||
                        shieldInfo.SubType == ItemSubType.Wings);
            }
        }

        private IPubFile<EIFRecord> EIFFile { get { return _eifFileProvider.EIFFile; } }
    }
}
