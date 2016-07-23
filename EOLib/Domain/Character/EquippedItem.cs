// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.IO;
using EOLib.IO.Extensions;
using EOLib.IO.Pub;

namespace EOLib.Domain.Character
{
    public class EquippedItem : IEquippedItem
    {
        public EquipLocation EquipLocation { get; private set; }

        public int ItemID { get; private set; }

        public EIFRecord ItemData { get; private set; }

        public EquippedItem(EIFRecord record)
        {
            if (record == null)
                throw new ArgumentNullException("record", "Item was not found in pub file (null record)");

            EquipLocation = record.GetEquipLocation();
            ItemID = record.ID;
            ItemData = record;
        }
    }
}