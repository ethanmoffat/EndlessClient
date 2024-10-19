using System.Collections.Generic;
using EOLib.IO;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.Domain.Extensions
{
    public static class PaperdollExtensions
    {
        public static IReadOnlyDictionary<EquipLocation, int> GetPaperdoll(this EquipmentWelcome equipment)
        {
            return new Dictionary<EquipLocation, int>
            {
                [EquipLocation.Boots] = equipment.Boots,
                [EquipLocation.Accessory] = equipment.Accessory,
                [EquipLocation.Gloves] = equipment.Gloves,
                [EquipLocation.Belt] = equipment.Belt,
                [EquipLocation.Armor] = equipment.Armor,
                [EquipLocation.Necklace] = equipment.Necklace,
                [EquipLocation.Hat] = equipment.Hat,
                [EquipLocation.Shield] = equipment.Shield,
                [EquipLocation.Weapon] = equipment.Weapon,
                [EquipLocation.Ring1] = equipment.Ring[0],
                [EquipLocation.Ring2] = equipment.Ring[1],
                [EquipLocation.Armlet1] = equipment.Armlet[0],
                [EquipLocation.Armlet2] = equipment.Armlet[1],
                [EquipLocation.Bracer1] = equipment.Bracer[0],
                [EquipLocation.Bracer2] = equipment.Bracer[1],
            };
        }

        public static IReadOnlyDictionary<EquipLocation, int> GetPaperdoll(this EquipmentPaperdoll equipment)
        {
            return new Dictionary<EquipLocation, int>
            {
                [EquipLocation.Boots] = equipment.Boots,
                [EquipLocation.Accessory] = equipment.Accessory,
                [EquipLocation.Gloves] = equipment.Gloves,
                [EquipLocation.Belt] = equipment.Belt,
                [EquipLocation.Armor] = equipment.Armor,
                [EquipLocation.Necklace] = equipment.Necklace,
                [EquipLocation.Hat] = equipment.Hat,
                [EquipLocation.Shield] = equipment.Shield,
                [EquipLocation.Weapon] = equipment.Weapon,
                [EquipLocation.Ring1] = equipment.Ring[0],
                [EquipLocation.Ring2] = equipment.Ring[1],
                [EquipLocation.Armlet1] = equipment.Armlet[0],
                [EquipLocation.Armlet2] = equipment.Armlet[1],
                [EquipLocation.Bracer1] = equipment.Bracer[0],
                [EquipLocation.Bracer2] = equipment.Bracer[1],
            };
        }
    }
}
