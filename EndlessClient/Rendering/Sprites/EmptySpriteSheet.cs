using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Sprites
{
    public class EmptySpriteSheet : ISpriteSheet
    {
        public bool HasTexture => false;
        public Texture2D SheetTexture => null;
        public Rectangle SourceRectangle { get; }

        public EmptySpriteSheet()
            : this(Rectangle.Empty) { }

        public EmptySpriteSheet(Rectangle sourceRectangle)
        {
            SourceRectangle = sourceRectangle;
        }

        public T[] GetSourceTextureData<T>() where T : struct
        {
            return Enumerable.Empty<T>().ToArray();
        }

        public Texture2D GetSourceTexture()
        {
            return SheetTexture;
        }
    }
}
