// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.Rendering.Sprites
{
    public interface ICharacterSpriteCalculator
    {
        ISpriteSheet GetBootsTexture(bool isBow);
        ISpriteSheet GetArmorTexture(bool isBow);
        ISpriteSheet GetHatTexture();
        ISpriteSheet GetShieldTexture(bool shieldIsOnBack);
        ISpriteSheet GetWeaponTexture(bool isBow);

        ISpriteSheet GetSkinTexture(bool isBow);
        ISpriteSheet GetHairTexture();
        ISpriteSheet GetFaceTexture();
        ISpriteSheet GetEmoteTexture();
    }
}
