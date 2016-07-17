// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Domain.Map
{
    public interface IMapItem
    {
        short UniqueID { get; }

        short ItemID { get; }

        byte X { get; }

        byte Y { get; }

        int Amount { get; }

        IMapItem WithAmount(int newAmount);
    }
}
