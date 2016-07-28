// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient.Rendering.Map
{
    public struct MapRenderBounds
    {
        public int FirstRow { get; private set; }
        public int LastRow { get; private set; }
        public int FirstCol { get; private set; }
        public int LastCol { get; private set; }

        public MapRenderBounds(int firstRow, int lastRow, int firstCol, int lastCol)
            : this()
        {
            FirstRow = firstRow;
            LastRow = lastRow;
            FirstCol = firstCol;
            LastCol = lastCol;
        }
    }
}
