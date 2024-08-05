using EOLib.IO.Pub;

namespace EOLib.IO.Extensions
{
    public static class EIFRecordExtensions
    {
        public static EquipLocation GetEquipLocation(this EIFRecord record)
        {
            switch (record.Type)
            {
                case ItemType.Accessory:
                    return EquipLocation.Accessory;
                case ItemType.Armlet:
                    return EquipLocation.Armlet1;
                case ItemType.Armor:
                    return EquipLocation.Armor;
                case ItemType.Belt:
                    return EquipLocation.Belt;
                case ItemType.Boots:
                    return EquipLocation.Boots;
                case ItemType.Bracer:
                    return EquipLocation.Bracer1;
                case ItemType.Gloves:
                    return EquipLocation.Gloves;
                case ItemType.Hat:
                    return EquipLocation.Hat;
                case ItemType.Necklace:
                    return EquipLocation.Necklace;
                case ItemType.Ring:
                    return EquipLocation.Ring1;
                case ItemType.Shield:
                    return EquipLocation.Shield;
                case ItemType.Weapon:
                    return EquipLocation.Weapon;
                default:
                    return EquipLocation.PAPERDOLL_MAX;
            }
        }
    }
}