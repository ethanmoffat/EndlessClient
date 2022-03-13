namespace EOLib.IO
{
    /// <summary>
    /// Enum representing the different properties that exist within the pub records
    /// </summary>
    /// <remarks>
    /// IPubRecord::Get uses reflection to resolve the correct property. Property value resolution is based on the naming of the values in this enum.
    /// These values should always match the names of the actual properties in the implementations of the IPubRecord files (plus the type prefix).
    /// To be safe, just don't change the names here unless you know what you're doing. It's a crappy design, but it checks out.
    /// </remarks>
    public enum PubRecordProperty
    {
        //Applicable to all records
        GlobalID,
        GlobalName,

        #region Item Specific

        ItemGraphic,
        ItemType,
        ItemSubType,

        ItemSpecial,
        ItemHP,
        ItemTP,
        ItemMinDam,
        ItemMaxDam,
        ItemAccuracy,
        ItemEvade,
        ItemArmor,

        ItemUnkByte19,

        ItemStr,
        ItemInt,
        ItemWis,
        ItemAgi,
        ItemCon,
        ItemCha,

        ItemLight,
        ItemDark,
        ItemEarth,
        ItemAir,
        ItemWater,
        ItemFire,

        ItemScrollMap,
        ItemDollGraphic,
        ItemExpReward,
        ItemHairColor,
        ItemEffect,
        ItemKey,

        ItemGender,
        ItemScrollX,

        ItemScrollY,
        ItemDualWieldDollGraphic,

        ItemLevelReq,
        ItemClassReq,
        ItemStrReq,
        ItemIntReq,
        ItemWisReq,
        ItemAgiReq,
        ItemConReq,
        ItemChaReq,

        ItemUnkElement,
        ItemUnkElementPower,

        ItemWeight,

        ItemUnkByte56,

        ItemSize,

        #endregion

        #region NPC Specific

        NPCGraphic,

        NPCUnkByte2,

        NPCBoss,
        NPCChild,
        NPCType,

        NPCUnkShort14,

        NPCVendorID,

        NPCHP,
        NPCExp,
        NPCMinDam,
        NPCMaxDam,

        NPCAccuracy,
        NPCEvade,
        NPCArmor,

        NPCUnkByte26,
        NPCUnkShort27,
        NPCUnkShort29,
        NPCElementWeak,
        NPCElementWeakPower,
        NPCUnkByte35,

        #endregion

        #region Spell Specific

        SpellShout,

        SpellIcon,
        SpellGraphic,

        SpellTP,
        SpellSP,

        SpellCastTime,

        SpellUnkByte9,
        SpellUnkByte10,

        SpellType,

        SpellUnkByte14,
        SpellUnkShort15,

        SpellTargetRestrict,
        SpellTarget,

        SpellUnkByte19,
        SpellUnkByte20,
        SpellUnkShort21,

        SpellMinDam,
        SpellMaxDam,
        SpellAccuracy,

        SpellUnkShort29,
        SpellUnkShort31,
        SpellUnkByte33,

        SpellHP,

        SpellUnkShort36,
        SpellUnkByte38,
        SpellUnkShort39,
        SpellUnkShort41,
        SpellUnkShort43,
        SpellUnkShort45,
        SpellUnkShort47,
        SpellUnkShort49,

        #endregion

        #region Class Specific

        ClassBase,
        ClassType,

        ClassStr,
        ClassInt,
        ClassWis,
        ClassAgi,
        ClassCon,
        ClassCha

        #endregion
    }
}
