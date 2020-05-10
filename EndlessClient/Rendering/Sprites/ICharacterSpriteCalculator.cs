using EOLib.Domain.Character;

namespace EndlessClient.Rendering.Sprites
{
    public interface ICharacterSpriteCalculator
    {
        ISpriteSheet GetBootsTexture(ICharacterRenderProperties _characterRenderProperties);
        ISpriteSheet GetArmorTexture(ICharacterRenderProperties _characterRenderProperties);
        ISpriteSheet GetHatTexture(ICharacterRenderProperties _characterRenderProperties);
        ISpriteSheet GetShieldTexture(ICharacterRenderProperties _characterRenderProperties);
        ISpriteSheet[] GetWeaponTextures(ICharacterRenderProperties _characterRenderProperties);

        ISpriteSheet GetSkinTexture(ICharacterRenderProperties _characterRenderProperties);
        ISpriteSheet GetHairTexture(ICharacterRenderProperties _characterRenderProperties);
        ISpriteSheet GetFaceTexture(ICharacterRenderProperties _characterRenderProperties);
        ISpriteSheet GetEmoteTexture(ICharacterRenderProperties _characterRenderProperties);
    }
}
