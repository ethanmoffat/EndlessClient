using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Sprites
{
    public class SpriteSheet : ISpriteSheet
    {
        public bool HasTexture => true;

        public Texture2D SheetTexture { get; }

        public Rectangle SourceRectangle { get; }

        public SpriteSheet(Texture2D texture)
        {
            SheetTexture = texture;
            SourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);
        }

        public SpriteSheet(Texture2D texture, Rectangle sourceArea)
        {
            SheetTexture = texture;
            SourceRectangle = sourceArea;
        }

        public T[] GetSourceTextureData<T>() where T : struct
        {
            var data = new T[SourceRectangle.Width * SourceRectangle.Height];
            SheetTexture.GetData(0, SourceRectangle, data, 0, data.Length);

            return data;
        }

        /// <summary>
        /// Get a new texture containing the data from SheetTexture within the bounds of SourceRectangle. Must be disposed.
        /// </summary>
        /// <returns>New texture containing just the image specified by the SourceRectangle property.</returns>
        public Texture2D GetSourceTexture()
        {
            var colorData = GetSourceTextureData<Color>();

            var retText = new Texture2D(SheetTexture.GraphicsDevice, SourceRectangle.Width, SourceRectangle.Height);
            retText.SetData(colorData);

            return retText;
        }
    }
}
