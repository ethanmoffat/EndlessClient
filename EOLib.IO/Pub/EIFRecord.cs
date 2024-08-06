using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public class EIFRecord : PubRecord
    {
        public short Graphic => Get<short>(PubRecordProperty.ItemGraphic);

        public ItemType Type => Get<ItemType>(PubRecordProperty.ItemType);
        public ItemSubType SubType => Get<ItemSubType>(PubRecordProperty.ItemSubType);

        public ItemSpecial Special => Get<ItemSpecial>(PubRecordProperty.ItemSpecial);
        public short HP => Get<short>(PubRecordProperty.ItemHP);
        public short TP => Get<short>(PubRecordProperty.ItemTP);
        public short MinDam => Get<short>(PubRecordProperty.ItemMinDam);
        public short MaxDam => Get<short>(PubRecordProperty.ItemMaxDam);
        public short Accuracy => Get<short>(PubRecordProperty.ItemAccuracy);
        public short Evade => Get<short>(PubRecordProperty.ItemEvade);
        public short Armor => Get<short>(PubRecordProperty.ItemArmor);

        public byte UnkByte19 => Get<byte>(PubRecordProperty.ItemUnkByte19);

        public byte Str => Get<byte>(PubRecordProperty.ItemStr);
        public byte Int => Get<byte>(PubRecordProperty.ItemInt);
        public byte Wis => Get<byte>(PubRecordProperty.ItemWis);
        public byte Agi => Get<byte>(PubRecordProperty.ItemAgi);
        public byte Con => Get<byte>(PubRecordProperty.ItemCon);
        public byte Cha => Get<byte>(PubRecordProperty.ItemCha);

        public byte Light => Get<byte>(PubRecordProperty.ItemLight);
        public byte Dark => Get<byte>(PubRecordProperty.ItemDark);
        public byte Earth => Get<byte>(PubRecordProperty.ItemEarth);
        public byte Air => Get<byte>(PubRecordProperty.ItemAir);
        public byte Water => Get<byte>(PubRecordProperty.ItemWater);
        public byte Fire => Get<byte>(PubRecordProperty.ItemFire);

        public int ScrollMap => Get<int>(PubRecordProperty.ItemScrollMap);
        public int DollGraphic => Get<int>(PubRecordProperty.ItemDollGraphic);
        public int ExpReward => Get<int>(PubRecordProperty.ItemExpReward);
        public int HairColor => Get<int>(PubRecordProperty.ItemHairColor);
        public int Effect => Get<int>(PubRecordProperty.ItemEffect);
        public int Key => Get<int>(PubRecordProperty.ItemKey);
        public int BeerPotency => Get<int>(PubRecordProperty.BeerPotency);

        public byte Gender => Get<byte>(PubRecordProperty.ItemGender);
        public byte ScrollX => Get<byte>(PubRecordProperty.ItemScrollX);

        public byte ScrollY => Get<byte>(PubRecordProperty.ItemScrollY);
        public byte DualWieldDollGraphic => Get<byte>(PubRecordProperty.ItemDualWieldDollGraphic);

        public short LevelReq => Get<short>(PubRecordProperty.ItemLevelReq);
        public short ClassReq => Get<short>(PubRecordProperty.ItemClassReq);
        public short StrReq => Get<short>(PubRecordProperty.ItemStrReq);
        public short IntReq => Get<short>(PubRecordProperty.ItemIntReq);
        public short WisReq => Get<short>(PubRecordProperty.ItemWisReq);
        public short AgiReq => Get<short>(PubRecordProperty.ItemAgiReq);
        public short ConReq => Get<short>(PubRecordProperty.ItemConReq);
        public short ChaReq => Get<short>(PubRecordProperty.ItemChaReq);

        public byte Element => Get<byte>(PubRecordProperty.ItemElement);
        public byte ElementPower => Get<byte>(PubRecordProperty.ItemElementPower);

        public byte Weight => Get<byte>(PubRecordProperty.ItemWeight);

        public byte UnkByte56 => Get<byte>(PubRecordProperty.ItemUnkByte56);

        public ItemSize Size => Get<ItemSize>(PubRecordProperty.ItemSize);

        public EIFRecord()
            : this(0, string.Empty)
        {
        }

        public EIFRecord(int id, string name)
            : base(id, name, PubRecordProperty.Item)
        {
        }

        private EIFRecord(int id, List<string> names, Dictionary<PubRecordProperty, RecordData> propertyBag)
            : base(id, names, propertyBag)
        {
        }

        protected override PubRecord MakeCopy(List<string> names, Dictionary<PubRecordProperty, RecordData> propertyBag)
        {
            return new EIFRecord(ID, new List<string>(names), new Dictionary<PubRecordProperty, RecordData>(propertyBag));
        }
    }
}
