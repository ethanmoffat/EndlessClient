// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO
{
    public enum PubRecordPropertyType
    {
        //Applicable to all records
        GlobalID,
        GlobalName,

        //Item-specific
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
    }
}
