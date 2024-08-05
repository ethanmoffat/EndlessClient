using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public class ESFRecord : PubRecord
    {
        public override int NumberOfNames => 2;

        public string Shout => Names[1];

        public short Icon => Get<short>(PubRecordProperty.SpellIcon);
        public short Graphic => Get<short>(PubRecordProperty.SpellGraphic);

        public short TP => Get<short>(PubRecordProperty.SpellTP);
        public short SP => Get<short>(PubRecordProperty.SpellSP);

        public byte CastTime => Get<byte>(PubRecordProperty.SpellCastTime);

        public byte UnkByte9 => Get<byte>(PubRecordProperty.SpellUnkByte9);
        public byte UnkByte10 => Get<byte>(PubRecordProperty.SpellUnkByte10);

        public SpellType Type => Get<SpellType>(PubRecordProperty.SpellType);

        public byte UnkByte14 => Get<byte>(PubRecordProperty.SpellUnkByte14);
        public short UnkShort15 => Get<short>(PubRecordProperty.SpellUnkShort15);

        public SpellTargetRestrict TargetRestrict => Get<SpellTargetRestrict>(PubRecordProperty.SpellTargetRestrict);
        public SpellTarget Target => Get<SpellTarget>(PubRecordProperty.SpellTarget);

        public byte UnkByte19 => Get<byte>(PubRecordProperty.SpellUnkByte19);
        public byte UnkByte20 => Get<byte>(PubRecordProperty.SpellUnkByte20);
        public short UnkShort21 => Get<short>(PubRecordProperty.SpellUnkShort21);

        public short MinDam => Get<short>(PubRecordProperty.SpellMinDam);
        public short MaxDam => Get<short>(PubRecordProperty.SpellMaxDam);
        public short Accuracy => Get<short>(PubRecordProperty.SpellAccuracy);

        public short UnkShort29 => Get<short>(PubRecordProperty.SpellUnkShort29);
        public short UnkShort31 => Get<short>(PubRecordProperty.SpellUnkShort31);
        public byte UnkByte33 => Get<byte>(PubRecordProperty.SpellUnkByte33);

        public short HP => Get<short>(PubRecordProperty.SpellHP);

        public short UnkShort36 => Get<short>(PubRecordProperty.SpellUnkShort36);
        public byte UnkByte38 => Get<byte>(PubRecordProperty.SpellUnkByte38);
        public short UnkShort39 => Get<short>(PubRecordProperty.SpellUnkShort39);
        public short UnkShort41 => Get<short>(PubRecordProperty.SpellUnkShort41);
        public short UnkShort43 => Get<short>(PubRecordProperty.SpellUnkShort43);
        public short UnkShort45 => Get<short>(PubRecordProperty.SpellUnkShort45);
        public short UnkShort47 => Get<short>(PubRecordProperty.SpellUnkShort47);
        public short UnkShort49 => Get<short>(PubRecordProperty.SpellUnkShort49);

        public ESFRecord()
            : this(0, string.Empty, string.Empty)
        {
        }

        public ESFRecord(int id, string name, string shout)
            : base(id, new List<string> { name, shout }, PubRecordProperty.Spell)
        {
        }

        private ESFRecord(int id, List<string> names, Dictionary<PubRecordProperty, RecordData> propertyBag)
            : base(id, names, propertyBag)
        {
        }

        protected override PubRecord MakeCopy(List<string> names, Dictionary<PubRecordProperty, RecordData> propertyBag)
        {
            return new ESFRecord(ID, new List<string>(names), new Dictionary<PubRecordProperty, RecordData>(propertyBag));
        }
    }
}
