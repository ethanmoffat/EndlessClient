// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO;

namespace EOLib.Domain.Character
{
    public class EquippedItem : IEquippedItem
    {
        public EquipLocation EquipLocation { get; private set; }

        public short ItemID { get; private set; }

        public EquippedItem(EquipLocation location, short itemID)
        {
            EquipLocation = location;
            ItemID = itemID;
        }
    }
}