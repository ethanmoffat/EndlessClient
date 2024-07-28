using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Sprites;

public interface ISpriteSheet
{
    bool HasTexture { get; }

    Texture2D SheetTexture { get; }

    Rectangle SourceRectangle { get; }

    T[] GetSourceTextureData<T>() where T : struct;

    Texture2D GetSourceTexture();
}