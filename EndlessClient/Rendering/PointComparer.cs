using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering
{
    public class PointComparer : IComparer<Point>
    {
        public int Compare(Point a, Point b)
        {
            if (a.Y < b.Y || a.X < b.X)
                return -1;
            if (a.Y > b.Y || a.X > b.X)
                return 1;
            return 0;
        }
    }
}