// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.NPC
{
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
