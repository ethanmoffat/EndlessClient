// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace EndlessClient
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
