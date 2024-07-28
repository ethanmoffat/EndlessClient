using System;

namespace EOLib
{
    public enum EODirection : byte
    {
        Down = 0,
        Left,
        Up,
        Right,
        Invalid = 255
    }

    public static class EODirectionExtensions
    {
        public static EODirection Opposite(this EODirection direction)
        {
            switch (direction)
            {
                case EODirection.Invalid: return EODirection.Invalid;
                default: return (EODirection)((int)(direction + 2) % 4);
            }
        }
    }
}