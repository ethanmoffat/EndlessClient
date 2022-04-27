using EndlessClient.Rendering.Sprites;
using EOLib.Domain.Character;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Character
{
    public interface ICharacterTextures
    {
        ISpriteSheet Boots { get; }
        ISpriteSheet Armor { get; }
        ISpriteSheet Hat { get; }
        ISpriteSheet Shield { get; }
        ISpriteSheet Weapon { get; }
        ISpriteSheet WeaponExtra { get; }

        ISpriteSheet Hair { get; }
        ISpriteSheet Skin { get; }

        ISpriteSheet Emote { get; }
        ISpriteSheet Face { get; }

        void Refresh(CharacterRenderProperties characterRenderProperties);
    }
}
