// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Diagnostics.CodeAnalysis;
using EOLib.IO.Extensions;
using EOLib.IO.Pub;
using NUnit.Framework;

namespace EOLib.IO.Test
{
    [TestFixture, ExcludeFromCodeCoverage]
    public class EIFRecordExtensionsTest
    {
        [Test]
        public void GetEquipLocation_Accessory_ReturnsAccessory()
        {
            Assert.AreEqual(EquipLocation.Accessory, new EIFRecord {Type = ItemType.Accessory}.GetEquipLocation());
        }

        [Test]
        public void GetEquipLocation_Armlet_ReturnsArmlet1()
        {
            Assert.AreEqual(EquipLocation.Armlet1, new EIFRecord { Type = ItemType.Armlet }.GetEquipLocation());
        }

        [Test]
        public void GetEquipLocation_Armor_ReturnsArmor()
        {
            Assert.AreEqual(EquipLocation.Armor, new EIFRecord { Type = ItemType.Armor }.GetEquipLocation());
        }

        [Test]
        public void GetEquipLocation_Belt_ReturnsBelt()
        {
            Assert.AreEqual(EquipLocation.Belt, new EIFRecord { Type = ItemType.Belt }.GetEquipLocation());
        }

        [Test]
        public void GetEquipLocation_Boots_ReturnsBoots()
        {
            Assert.AreEqual(EquipLocation.Boots, new EIFRecord { Type = ItemType.Boots }.GetEquipLocation());
        }

        [Test]
        public void GetEquipLocation_Bracer_ReturnsBracer()
        {
            Assert.AreEqual(EquipLocation.Bracer1, new EIFRecord { Type = ItemType.Bracer }.GetEquipLocation());
        }

        [Test]
        public void GetEquipLocation_Gloves_ReturnsGloves()
        {
            Assert.AreEqual(EquipLocation.Gloves, new EIFRecord { Type = ItemType.Gloves }.GetEquipLocation());
        }

        [Test]
        public void GetEquipLocation_Hat_ReturnsHat()
        {
            Assert.AreEqual(EquipLocation.Hat, new EIFRecord { Type = ItemType.Hat }.GetEquipLocation());
        }

        [Test]
        public void GetEquipLocation_Necklace_ReturnsNecklace()
        {
            Assert.AreEqual(EquipLocation.Necklace, new EIFRecord { Type = ItemType.Necklace }.GetEquipLocation());
        }

        [Test]
        public void GetEquipLocation_Ring_ReturnsRing1()
        {
            Assert.AreEqual(EquipLocation.Ring1, new EIFRecord { Type = ItemType.Ring }.GetEquipLocation());
        }

        [Test]
        public void GetEquipLocation_Shield_ReturnsShield()
        {
            Assert.AreEqual(EquipLocation.Shield, new EIFRecord { Type = ItemType.Shield }.GetEquipLocation());
        }

        [Test]
        public void GetEquipLocation_Weapon_ReturnsWeapon()
        {
            Assert.AreEqual(EquipLocation.Weapon, new EIFRecord { Type = ItemType.Weapon }.GetEquipLocation());
        }

        [Test]
        public void GetEquipLocation_Unsupported_ReturnsPaperdollMax()
        {
            var unsupported = new[]
            {
                ItemType.Beer,
                ItemType.CureCurse,
                ItemType.EXPReward,
                ItemType.EffectPotion,
                ItemType.HairDye,
                ItemType.Heal,
                ItemType.Key,
                ItemType.Money,
                ItemType.SkillReward,
                ItemType.StatReward,
                ItemType.Static,
                ItemType.Teleport,
                ItemType.UnknownType1
            };

            foreach (var type in unsupported)
                Assert.AreEqual(EquipLocation.PAPERDOLL_MAX, new EIFRecord {Type = type}.GetEquipLocation());
        }
    }
}
