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
        Texture2D Boots { get; }
        ISpriteSheet Armor { get; }
        ISpriteSheet Hat { get; }
        Texture2D Shield { get; }
        Texture2D Weapon { get; }

        ISpriteSheet Hair { get; }
        ISpriteSheet Skin { get; }

        ISpriteSheet Emote { get; }
        ISpriteSheet Face { get; }

        void Refresh(ICharacterRenderProperties characterRenderProperties);
    }
}
