// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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

        void Refresh(ICharacterRenderProperties characterRenderProperties);
    }
}
