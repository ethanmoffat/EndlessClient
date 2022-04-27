using AutomaticTypeMapper;
using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Character
{
    [MappedType(BaseType = typeof(ICharacterTextures))]
    public class CharacterTextures : ICharacterTextures
    {
        private readonly ICharacterSpriteCalculator _characterSpriteCalculator;
        public ISpriteSheet Boots { get; private set; }
        public ISpriteSheet Armor { get; private set; }
        public ISpriteSheet Hat { get; private set; }
        public ISpriteSheet Shield { get; private set; }
        public ISpriteSheet Weapon { get; private set; }
        public ISpriteSheet WeaponExtra { get; private set; }

        public ISpriteSheet Hair { get; private set; }
        public ISpriteSheet Skin { get; private set; }

        public ISpriteSheet Emote { get; private set; }
        public ISpriteSheet Face { get; private set; }

        public CharacterTextures(ICharacterSpriteCalculator characterSpriteCalculator)
        {
            _characterSpriteCalculator = characterSpriteCalculator;
        }

        public void Refresh(CharacterRenderProperties characterRenderProperties)
        {
            Boots = _characterSpriteCalculator.GetBootsTexture(characterRenderProperties);
            Armor = _characterSpriteCalculator.GetArmorTexture(characterRenderProperties);
            Hat = _characterSpriteCalculator.GetHatTexture(characterRenderProperties);
            Shield = _characterSpriteCalculator.GetShieldTexture(characterRenderProperties);

            var weaponTextures = _characterSpriteCalculator.GetWeaponTextures(characterRenderProperties);
            Weapon = weaponTextures[0];
            WeaponExtra = weaponTextures[1];

            Hair = _characterSpriteCalculator.GetHairTexture(characterRenderProperties);
            Skin = _characterSpriteCalculator.GetSkinTexture(characterRenderProperties);
            Emote = _characterSpriteCalculator.GetEmoteTexture(characterRenderProperties);
            Face = _characterSpriteCalculator.GetFaceTexture(characterRenderProperties);
        }
    }
}
