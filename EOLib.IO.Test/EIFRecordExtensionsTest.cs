using System.Diagnostics.CodeAnalysis;
using EOLib.IO.Extensions;
using EOLib.IO.Pub;
using NUnit.Framework;

namespace EOLib.IO.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class EIFRecordExtensionsTest
    {
        [TestCase(EquipLocation.Accessory, ItemType.Accessory)]
        [TestCase(EquipLocation.Armlet1, ItemType.Armlet)]
        [TestCase(EquipLocation.Armor, ItemType.Armor)]
        [TestCase(EquipLocation.Belt, ItemType.Belt)]
        [TestCase(EquipLocation.Boots, ItemType.Boots)]
        [TestCase(EquipLocation.Bracer1, ItemType.Bracer)]
        [TestCase(EquipLocation.Gloves, ItemType.Gloves)]
        [TestCase(EquipLocation.Hat, ItemType.Hat)]
        [TestCase(EquipLocation.Necklace, ItemType.Necklace)]
        [TestCase(EquipLocation.Ring1, ItemType.Ring)]
        [TestCase(EquipLocation.Shield, ItemType.Shield)]
        [TestCase(EquipLocation.Weapon, ItemType.Weapon)]
        public void GetEquipLocation_Matches_ItemType(EquipLocation equipLocation, ItemType itemType)
        {
            Assert.That(WithItemType(itemType).GetEquipLocation(), Is.EqualTo(equipLocation));
        }

        [TestCase(ItemType.Beer)]
        [TestCase(ItemType.CureCurse)]
        [TestCase(ItemType.EXPReward)]
        [TestCase(ItemType.EffectPotion)]
        [TestCase(ItemType.HairDye)]
        [TestCase(ItemType.Heal)]
        [TestCase(ItemType.Key)]
        [TestCase(ItemType.Money)]
        [TestCase(ItemType.SkillReward)]
        [TestCase(ItemType.StatReward)]
        [TestCase(ItemType.Static)]
        [TestCase(ItemType.Teleport)]
        [TestCase(ItemType.UnknownType1)]
        public void GetEquipLocation_Unsupported_ReturnsPaperdollMax(ItemType type)
        {
            Assert.That(WithItemType(type).GetEquipLocation(), Is.EqualTo(EquipLocation.PAPERDOLL_MAX));
        }

        private static EIFRecord WithItemType(ItemType type) => (EIFRecord)new EIFRecord().WithProperty(PubRecordProperty.ItemType, (int)type);
    }
}
