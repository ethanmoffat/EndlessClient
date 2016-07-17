// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Character
{
    public interface IInventorySpell
    {
        short ID { get; }

        short Level { get; }

        IInventorySpell WithLevel(short newLevel);
    }
}