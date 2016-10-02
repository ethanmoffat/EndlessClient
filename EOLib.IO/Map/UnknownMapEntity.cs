// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.IO.Map
{
    public class UnknownMapEntity : IMapEntity
    {
        public const int DATA_SIZE = 4;

        public int X { get; private set; }

        public int Y { get; private set; }

        public byte[] RawData { get; private set; }

        public UnknownMapEntity()
            : this(-1, -1, null)
        { }

        private UnknownMapEntity(int x, int y, byte[] rawData)
        {
            X = x;
            Y = y;
            RawData = rawData;
        }

        public UnknownMapEntity WithX(int x)
        {
            var newEntity = MakeCopy(this);
            newEntity.X = x;
            return newEntity;
        }

        public UnknownMapEntity WithY(int y)
        {
            var newEntity = MakeCopy(this);
            newEntity.Y = y;
            return newEntity;
        }

        public UnknownMapEntity WithRawData(byte[] rawData)
        {
            var newEntity = MakeCopy(this);
            newEntity.RawData = rawData;
            return newEntity;
        }

        private static UnknownMapEntity MakeCopy(UnknownMapEntity src)
        {
            var copy = new byte[src.RawData.Length];
            Array.Copy(src.RawData, copy, copy.Length);
            return new UnknownMapEntity(src.X, src.Y, copy);
        }
    }
}
