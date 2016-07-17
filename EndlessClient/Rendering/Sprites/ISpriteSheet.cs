// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Sprites
{
    public interface ISpriteSheet
    {
        bool HasTexture { get; }

        Texture2D SheetTexture { get; }

        Rectangle SourceRectangle { get; }

        T[] GetSourceTextureData<T>() where T : struct;

        Texture2D GetSourceTexture();
    }
}
