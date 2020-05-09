using System;

namespace EOLib.Graphics
{
    public struct LibraryGraphicPair : IComparable
    {
        private readonly int LibraryNumber;
        private readonly int GraphicNumber;

        public LibraryGraphicPair(int lib, int gfx)
        {
            LibraryNumber = lib;
            GraphicNumber = gfx;
        }

        public int CompareTo(object other)
        {
            if (!(other is LibraryGraphicPair))
                return -1;

            LibraryGraphicPair rhs = (LibraryGraphicPair)other;

            if (rhs.LibraryNumber == LibraryNumber && rhs.GraphicNumber == GraphicNumber)
                return 0;

            return -1;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LibraryGraphicPair)) return false;
            LibraryGraphicPair other = (LibraryGraphicPair)obj;
            return other.GraphicNumber == GraphicNumber && other.LibraryNumber == LibraryNumber;
        }

        public override int GetHashCode()
        {
            return (LibraryNumber << 16) | GraphicNumber;
        }
    }
}
