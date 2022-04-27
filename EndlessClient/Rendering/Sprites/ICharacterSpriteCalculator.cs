using EOLib.Domain.Character;

namespace EndlessClient.Rendering.Sprites
{
    public interface ICharacterSpriteCalculator
    {
        ISpriteSheet GetBootsTexture(CharacterRenderProperties _characterRenderProperties);
        ISpriteSheet GetArmorTexture(CharacterRenderProperties _characterRenderProperties);
        ISpriteSheet GetHatTexture(CharacterRenderProperties _characterRenderProperties);
        ISpriteSheet GetShieldTexture(CharacterRenderProperties _characterRenderProperties);
        ISpriteSheet[] GetWeaponTextures(CharacterRenderProperties _characterRenderProperties);

        ISpriteSheet GetSkinTexture(CharacterRenderProperties _characterRenderProperties);
        ISpriteSheet GetHairTexture(CharacterRenderProperties _characterRenderProperties);
        ISpriteSheet GetFaceTexture(CharacterRenderProperties _characterRenderProperties);
        ISpriteSheet GetEmoteTexture(CharacterRenderProperties _characterRenderProperties);
    }
}
