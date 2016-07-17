// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;

namespace EOLib.Graphics
{
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
    }
}