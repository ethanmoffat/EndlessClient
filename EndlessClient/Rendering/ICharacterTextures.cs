// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Sprites;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering
{
	public interface ICharacterTextures
	{
		Texture2D Boots { get; }
		Texture2D Armor { get; }
		Texture2D Hat { get; }
		Texture2D Shield { get; }
		Texture2D Weapon { get; }

		Texture2D Hair { get; }
		ISpriteSheet Skin { get; }

		ISpriteSheet Emote { get; }
		ISpriteSheet Face { get; }

		void RefreshTextures(bool bowIsEquipped, bool shieldIsOnBack);
	}
}
