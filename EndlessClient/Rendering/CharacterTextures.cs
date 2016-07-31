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

        public Texture2D Boots { get; private set; }
        public Texture2D Armor { get; private set; }
        public Texture2D Hat { get; private set; }
        public Texture2D Shield { get; private set; }
        public Texture2D Weapon { get; private set; }

        public Texture2D Hair { get; private set; }
        public ISpriteSheet Skin { get; private set; }

        public ISpriteSheet Emote { get; private set; }
        public ISpriteSheet Face { get; private set; }

        public CharacterTextures(IEIFFileProvider eifFileProvider)
        {
            _eifFileProvider = eifFileProvider;
        }

        public void Refresh(ICharacterSpriteCalculator calc,
                            ICharacterRenderProperties characterRenderProperties)
        {
            Boots = calc.GetBootsTexture(BowIsEquipped(characterRenderProperties)).SheetTexture;
            Armor = calc.GetArmorTexture(BowIsEquipped(characterRenderProperties)).SheetTexture;
            Hat = calc.GetHatTexture().SheetTexture;
            Shield = calc.GetShieldTexture(ShieldIsOnBack(characterRenderProperties)).SheetTexture;
            Weapon = calc.GetWeaponTexture(BowIsEquipped(characterRenderProperties)).SheetTexture;

            Hair = calc.GetHairTexture().SheetTexture;
            Skin = calc.GetSkinTexture(BowIsEquipped(characterRenderProperties));
            Emote = calc.GetEmoteTexture();
            Face = calc.GetFaceTexture();
        }

        private bool BowIsEquipped(ICharacterRenderProperties characterRenderProperties)
        {
            if (EIFFile == null || EIFFile.Data == null)
                return false;

            var itemData = EIFFile.Data;
            var weaponInfo = itemData.SingleOrDefault(x => x.Type == ItemType.Weapon &&
                                                            x.DollGraphic == characterRenderProperties.WeaponGraphic);

            return weaponInfo != null && weaponInfo.SubType == ItemSubType.Ranged;
        }

        private bool ShieldIsOnBack(ICharacterRenderProperties characterRenderProperties)
        {
            if (EIFFile == null || EIFFile.Data == null)
                return false;

            var itemData = EIFFile.Data;
            var shieldInfo = itemData.SingleOrDefault(x => x.Type == ItemType.Shield &&
                                                            x.DollGraphic == characterRenderProperties.ShieldGraphic);

            return shieldInfo != null &&
                    (shieldInfo.Name == "Bag" ||
                    shieldInfo.SubType == ItemSubType.Arrows ||
                    shieldInfo.SubType == ItemSubType.Wings);
        }

        private IPubFile<EIFRecord> EIFFile { get { return _eifFileProvider.EIFFile; } }
    }
}
