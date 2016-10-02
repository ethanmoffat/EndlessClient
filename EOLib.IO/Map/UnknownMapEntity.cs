// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Map
{
    public class UnknownMapEntity : IMapEntity
    {
        public const int DATA_SIZE = 4;

        public int X { get; set; }

        public int Y { get; set; }

        public byte[] RawData { get; set; }
    }
}
