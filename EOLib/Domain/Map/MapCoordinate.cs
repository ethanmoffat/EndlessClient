using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System;

namespace EOLib.Domain.Map
{
    public readonly struct MapCoordinate : IComparable<MapCoordinate>
    {
        public static MapCoordinate Zero { get; } = new MapCoordinate(0, 0);

        public static MapCoordinate Max { get; } = new MapCoordinate(255, 255);

        public int X { get; }

        public int Y { get; }

        public MapCoordinate(BigCoords coords)
        {
            X = coords.X;
            Y = coords.Y;
        }

        public MapCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static MapCoordinate operator -(MapCoordinate lhs, MapCoordinate rhs)
        {
            return new MapCoordinate(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static bool operator ==(MapCoordinate left, MapCoordinate right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MapCoordinate left, MapCoordinate right)
        {
            return !(left == right);
        }

        public override string ToString() => $"{X}, {Y}";

        public override bool Equals(object obj)
        {
            if (!(obj is MapCoordinate))
                return false;

            var other = (MapCoordinate)obj;
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            var hash = 397 ^ X.GetHashCode();
            hash = (hash * 397) ^ Y.GetHashCode();
            return hash;
        }

        public int CompareTo(MapCoordinate other)
        {
            if (other == null)
                return -1;

            if (other.X < X || other.Y < Y)
                return -1;

            if (other.X > X || other.Y > Y)
                return 1;

            return 0;
        }
    }
}