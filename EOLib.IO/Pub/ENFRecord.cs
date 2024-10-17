using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public class ENFRecord : PubRecord
    {
        public int Graphic => Get<int>(PubRecordProperty.NPCGraphic);

        public byte UnkByte2 => Get<byte>(PubRecordProperty.NPCUnkByte2);

        public short Boss => Get<short>(PubRecordProperty.NPCBoss);
        public short Child => Get<short>(PubRecordProperty.NPCChild);
        public NPCType Type => Get<NPCType>(PubRecordProperty.NPCType);

        public short UnkShort14 => Get<short>(PubRecordProperty.NPCUnkShort14);

        public short VendorID => Get<short>(PubRecordProperty.NPCVendorID);

        public int HP => Get<int>(PubRecordProperty.NPCHP);

        public short MinDam => Get<short>(PubRecordProperty.NPCMinDam);
        public short MaxDam => Get<short>(PubRecordProperty.NPCMaxDam);

        public short Accuracy => Get<short>(PubRecordProperty.NPCAccuracy);
        public short Evade => Get<short>(PubRecordProperty.NPCEvade);
        public short Armor => Get<short>(PubRecordProperty.NPCArmor);

        public byte UnkByte26 => Get<byte>(PubRecordProperty.NPCUnkByte26);
        public short UnkShort27 => Get<short>(PubRecordProperty.NPCUnkShort27);
        public short UnkShort29 => Get<short>(PubRecordProperty.NPCUnkShort29);

        public short ElementWeak => Get<short>(PubRecordProperty.NPCElementWeak);
        public short ElementWeakPower => Get<short>(PubRecordProperty.NPCElementWeakPower);

        public byte UnkByte35 => Get<byte>(PubRecordProperty.NPCUnkByte35);

        public int Exp => Get<int>(PubRecordProperty.NPCExp);

        public ENFRecord()
            : this(0, string.Empty)
        {
        }

        public ENFRecord(int id, string name)
            : base(id, name, PubRecordProperty.NPC)
        {
        }

        private ENFRecord(int id, List<string> names, Dictionary<PubRecordProperty, RecordData> propertyBag)
            : base(id, names, propertyBag)
        {
        }

        protected override PubRecord MakeCopy(List<string> names, Dictionary<PubRecordProperty, RecordData> propertyBag)
        {
            return new ENFRecord(ID, new List<string>(names), new Dictionary<PubRecordProperty, RecordData>(propertyBag));
        }
    }
}
