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

        ItemWeight,

        ItemSize,

        #endregion

        #region NPC Specific

        NPCGraphic,

        NPCBoss,
        NPCChild,
        NPCType,

        NPCVendorID,

        NPCHP,
        NPCExp,
        NPCMinDam,
        NPCMaxDam,

        NPCAccuracy,
        NPCEvade,
        NPCArmor,

        #endregion

        #region Spell Specific

        SpellShout,

        SpellIcon,
        SpellGraphic,

        SpellTP,
        SpellSP,

        SpellCastTime,

        SpellType,
        SpellTargetRestrict,
        SpellTarget,

        SpellMinDam,
        SpellMaxDam,
        SpellAccuracy,
        SpellHP,

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
