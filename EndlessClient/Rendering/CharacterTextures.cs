// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Sprites;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering
{
	public class CharacterTextures : ICharacterTextures
	{
		private readonly ICharacterSpriteCalculator _calc;

		public Texture2D Boots { get; private set; }
		public Texture2D Armor { get; private set; }
		public Texture2D Hat { get; private set; }
		public Texture2D Shield { get; private set; }
		public Texture2D Weapon { get; private set; }

		public Texture2D Hair { get; private set; }
		public ISpriteSheet Skin { get; private set; }

		public ISpriteSheet Emote { get; private set; }
		public ISpriteSheet Face { get; private set; }

		public CharacterTextures(ICharacterSpriteCalculator calc)
		{
			_calc = calc;
		}

		public void RefreshTextures(bool bowIsEquipped, bool shieldIsOnBack)
		{
			Boots = _calc.GetBootsTexture(bowIsEquipped).SheetTexture;
			Armor = _calc.GetArmorTexture(bowIsEquipped).SheetTexture;
			Hat = _calc.GetHatTexture().SheetTexture;
			Shield = _calc.GetShieldTexture(shieldIsOnBack).SheetTexture;
			Weapon = _calc.GetWeaponTexture(bowIsEquipped).SheetTexture;

			Hair = _calc.GetHairTexture().SheetTexture;
			Skin = _calc.GetSkinTexture(bowIsEquipped);
			Emote = _calc.GetEmoteTexture();
			Face = _calc.GetFaceTexture();
		}
	}
}
