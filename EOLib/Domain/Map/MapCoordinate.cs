// Original Work Copyright (c) Ethan Moffat 2014-2019

namespace EOLib.Domain.Map
{
    public struct MapCoordinate
    {
        public int X { get; }

        public int Y { get; }

        public MapCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"{X}, {Y}";

        public override bool Equals(object obj)
        {
            if (!(obj is MapCoordinate))
                return false;

            var other = (MapCoordinate) obj;
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            var hash = 397 ^ X.GetHashCode();
            hash = (hash * 397) ^ Y.GetHashCode();
            return hash;
        }
    }
}
