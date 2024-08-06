using System;

namespace EOLib.IO.Pub
{
    /// <summary>
    /// Enum representing the different properties that exist within the pub records
    /// </summary>
    [Flags]
    public enum PubRecordProperty : uint
    {
        Item = 0x100,

        [RecordData(0, 2)]
        ItemGraphic,
        [RecordData(2, 1)]
        ItemType,
        [RecordData(3, 1)]
        ItemSubType,

        [RecordData(4, 1)]
        ItemSpecial,
        [RecordData(5, 2)]
        ItemHP,
        [RecordData(7, 2)]
        ItemTP,
        [RecordData(9, 2)]
        ItemMinDam,
        [RecordData(11, 2)]
        ItemMaxDam,
        [RecordData(13, 2)]
        ItemAccuracy,
        [RecordData(15, 2)]
        ItemEvade,
        [RecordData(17, 2)]
        ItemArmor,

        [RecordData(19, 1)]
        ItemUnkByte19,

        [RecordData(20, 1)]
        ItemStr,
        [RecordData(21, 1)]
        ItemInt,
        [RecordData(22, 1)]
        ItemWis,
        [RecordData(23, 1)]
        ItemAgi,
        [RecordData(24, 1)]
        ItemCon,
        [RecordData(25, 1)]
        ItemCha,

        [RecordData(26, 1)]
        ItemLight, // elements for items are potentially mislabeled
        [RecordData(27, 1)]
        ItemDark,
        [RecordData(28, 1)]
        ItemEarth,
        [RecordData(29, 1)]
        ItemAir,
        [RecordData(30, 1)]
        ItemWater,
        [RecordData(31, 1)]
        ItemFire,

        [RecordData(32, 3)]
        ItemScrollMap,
        [RecordData(32, 3)]
        ItemDollGraphic,
        [RecordData(32, 3)]
        ItemExpReward,
        [RecordData(32, 3)]
        ItemHairColor,
        [RecordData(32, 3)]
        ItemEffect,
        [RecordData(32, 3)]
        ItemKey,
        [RecordData(32, 3)]
        BeerPotency,

        [RecordData(35, 1)]
        ItemGender,
        [RecordData(35, 1)]
        ItemScrollX,

        [RecordData(36, 1)]
        ItemScrollY,
        [RecordData(36, 1)]
        ItemDualWieldDollGraphic,

        [RecordData(37, 2)]
        ItemLevelReq,
        [RecordData(39, 2)]
        ItemClassReq,
        [RecordData(41, 2)]
        ItemStrReq,
        [RecordData(43, 2)]
        ItemIntReq,
        [RecordData(45, 2)]
        ItemWisReq,
        [RecordData(47, 2)]
        ItemAgiReq,
        [RecordData(49, 2)]
        ItemConReq,
        [RecordData(51, 2)]
        ItemChaReq,

        [RecordData(53, 1)]
        ItemElement,
        [RecordData(54, 1)]
        ItemElementPower,

        [RecordData(55, 1)]
        ItemWeight,

        [RecordData(56, 1)]
        ItemUnkByte56,

        [RecordData(57, 1)]
        ItemSize,

        NPC = 0x200,

        [RecordData(0, 2)]
        NPCGraphic,

        [RecordData(2, 1)]
        NPCUnkByte2,

        [RecordData(3, 2)]
        NPCBoss,
        [RecordData(5, 2)]
        NPCChild,
        [RecordData(7, 2)]
        NPCType,
        [RecordData(9, 2)]
        NPCVendorID,

        [RecordData(11, 3)]
        NPCHP,

        [RecordData(14, 2)]
        NPCUnkShort14,

        [RecordData(16, 2)]
        NPCMinDam,
        [RecordData(18, 2)]
        NPCMaxDam,

        [RecordData(20, 2)]
        NPCAccuracy,
        [RecordData(22, 2)]
        NPCEvade,
        [RecordData(24, 2)]
        NPCArmor,

        [RecordData(26, 1)]
        NPCUnkByte26,
        [RecordData(27, 2)]
        NPCUnkShort27, // potentially NPCElementStrong
        [RecordData(29, 2)]
        NPCUnkShort29, // potentially NPCElementStrongPower

        [RecordData(31, 2)]
        NPCElementWeak,
        [RecordData(33, 2)]
        NPCElementWeakPower,
        [RecordData(35, 1)]
        NPCUnkByte35,

        [RecordData(36, 3)]
        NPCExp,

        Spell = 0x400,

        [RecordData(0, 2)]
        SpellIcon,
        [RecordData(2, 2)]
        SpellGraphic,

        [RecordData(4, 2)]
        SpellTP,
        [RecordData(6, 2)]
        SpellSP,

        [RecordData(8, 1)]
        SpellCastTime,

        [RecordData(9, 1)]
        SpellUnkByte9,
        [RecordData(10, 1)]
        SpellUnkByte10,

        [RecordData(11, 3)]
        SpellType, // eoserv reads this as a single byte

        [RecordData(14, 1)]
        SpellUnkByte14, // potentially element
        [RecordData(15, 2)]
        SpellUnkShort15, // potentially element power

        [RecordData(17, 1)]
        SpellTargetRestrict,
        [RecordData(18, 1)]
        SpellTarget,

        [RecordData(19, 1)]
        SpellUnkByte19,
        [RecordData(20, 1)]
        SpellUnkByte20,
        [RecordData(21, 2)]
        SpellUnkShort21,

        [RecordData(23, 2)]
        SpellMinDam,
        [RecordData(25, 2)]
        SpellMaxDam,
        [RecordData(27, 2)]
        SpellAccuracy,

        [RecordData(29, 2)]
        SpellUnkShort29,
        [RecordData(31, 2)]
        SpellUnkShort31,
        [RecordData(33, 1)]
        SpellUnkByte33,

        [RecordData(34, 2)]
        SpellHP,

        [RecordData(36, 2)]
        SpellUnkShort36, // potentially TP heal
        [RecordData(38, 1)]
        SpellUnkByte38, // potentially SP heal
        [RecordData(39, 2)]
        SpellUnkShort39,
        [RecordData(41, 2)]
        SpellUnkShort41,
        [RecordData(43, 2)]
        SpellUnkShort43,
        [RecordData(45, 2)]
        SpellUnkShort45,
        [RecordData(47, 2)]
        SpellUnkShort47,
        [RecordData(49, 2)]
        SpellUnkShort49,

        Class = 0x800,

        [RecordData(0, 1)]
        ClassBase,
        [RecordData(1, 1)]
        ClassType,

        [RecordData(2, 2)]
        ClassStr,
        [RecordData(4, 2)]
        ClassInt,
        [RecordData(6, 2)]
        ClassWis,
        [RecordData(8, 2)]
        ClassAgi,
        [RecordData(10, 2)]
        ClassCon,
        [RecordData(12, 2)]
        ClassCha
    }
}
