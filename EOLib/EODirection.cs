// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

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
                case EODirection.Down: return EODirection.Up;
                case EODirection.Left: return EODirection.Right;
                case EODirection.Up: return EODirection.Down;
                case EODirection.Right: return EODirection.Left;
                case EODirection.Invalid: return EODirection.Invalid;
                default: throw new ArgumentOutOfRangeException("direction", direction, null);
            }
        }
    }
}