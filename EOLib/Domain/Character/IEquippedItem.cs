// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO;

namespace EOLib.Domain.Character
{
    public interface IEquippedItem
    {
        EquipLocation EquipLocation { get; }

        short ItemID { get; }
    }
}