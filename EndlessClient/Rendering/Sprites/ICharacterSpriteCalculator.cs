using EOLib.Domain.Character;

namespace EndlessClient.Rendering.Sprites
{
    public interface ICharacterSpriteCalculator
    {
        ISpriteSheet GetBootsTexture(CharacterRenderProperties characterRenderProperties);
        ISpriteSheet GetArmorTexture(CharacterRenderProperties characterRenderProperties);
        ISpriteSheet GetHatTexture(CharacterRenderProperties characterRenderProperties);
        ISpriteSheet GetShieldTexture(CharacterRenderProperties characterRenderProperties);
        ISpriteSheet[] GetWeaponTextures(CharacterRenderProperties characterRenderProperties);
        ISpriteSheet GetWeaponSlash(CharacterRenderProperties characterRenderProperties);

        ISpriteSheet GetSkinTexture(CharacterRenderProperties characterRenderProperties);
        ISpriteSheet GetHairTexture(CharacterRenderProperties characterRenderProperties);
        ISpriteSheet GetFaceTexture(CharacterRenderProperties characterRenderProperties);
        ISpriteSheet GetEmoteTexture(CharacterRenderProperties characterRenderProperties);
    }
}
