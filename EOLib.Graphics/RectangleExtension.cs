using Microsoft.Xna.Framework;

namespace EOLib.Graphics;

public static class RectangleExtension
{
    /// <summary>
    /// Returns a new rectangle with the position set to the specified location
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="loc">New position for the rectangle</param>
    /// <returns></returns>
    public static Rectangle WithPosition(this Rectangle orig, Vector2 loc)
    {
        return new Rectangle((int)loc.X, (int)loc.Y, orig.Width, orig.Height);
    }

    /// <summary>
    /// Returns a new rectangle with the size set to the specified dimensions
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="width">Width of the new rectangle</param>
    /// <param name="height">Height of the new rectangle</param>
    /// <returns></returns>
    public static Rectangle WithSize(this Rectangle orig, int width, int height)
    {
        return new Rectangle(orig.X, orig.Y, width, height);
    }
}