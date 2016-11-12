// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Map
{
    public class MapNPC : IMapNPC
    {
        public short ID { get; private set; }

        public byte Index { get; private set; }

        public byte X { get; private set; }

        public byte Y { get; private set; }

        public EODirection Direction { get; private set; }

        public MapNPC(short id, byte index)
        {
            ID = id;
            Index = index;
        }

        public IMapNPC WithX(byte x)
        {
            var copy = MakeCopy(this);
            copy.X = x;
            return copy;
        }

        public IMapNPC WithY(byte y)
        {
            var copy = MakeCopy(this);
            copy.Y = y;
            return copy;
        }

        public IMapNPC WithDirection(EODirection direction)
        {
            var copy = MakeCopy(this);
            copy.Direction = direction;
            return copy;
        }

        private static MapNPC MakeCopy(IMapNPC input)
        {
            return new MapNPC(input.ID, input.Index)
            {
                X = input.X,
                Y = input.Y,
                Direction = input.Direction
            };
        }
    }

    public interface IMapNPC
    {
        short ID { get; }

        byte Index { get; }

        byte X { get; }

        byte Y { get; }

        EODirection Direction { get; }

        IMapNPC WithX(byte x);

        IMapNPC WithY(byte y);

        IMapNPC WithDirection(EODirection direction);
    }
}