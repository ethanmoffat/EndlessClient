using System;
using EOLib.IO;
using EOLib.IO.Extensions;
using EOLib.IO.Pub;

namespace EOLib.Domain.Character
{
    public class EquippedItem : IEquippedItem
    {
        public EquipLocation EquipLocation { get; }

        public int ItemID { get; }

        public EIFRecord ItemData { get; }

        public EquippedItem(EIFRecord record)
        {
            if (record == null)
                throw new ArgumentNullException(nameof(record), "Item was not found in pub file (null record)");

            EquipLocation = record.GetEquipLocation();
            ItemID = record.ID;
            ItemData = record;
        }
    }

    public interface IEquippedItem
    {
        EquipLocation EquipLocation { get; }

        int ItemID { get; }

        EIFRecord ItemData { get; }
    }
}
