using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EOLib.IO.Services;

namespace EOLib.IO.Pub
{
    public class ECFRecord : PubRecord
    {
        public byte Base => Get<byte>(PubRecordProperty.ClassBase);
        public byte Type => Get<byte>(PubRecordProperty.ClassType);

        public short Str => Get<short>(PubRecordProperty.ClassStr);
        public short Int => Get<short>(PubRecordProperty.ClassInt);
        public short Wis => Get<short>(PubRecordProperty.ClassWis);
        public short Agi => Get<short>(PubRecordProperty.ClassAgi);
        public short Con => Get<short>(PubRecordProperty.ClassCon);
        public short Cha => Get<short>(PubRecordProperty.ClassCha);

        public ECFRecord()
            : this(0, string.Empty)
        {
        }

        public ECFRecord(int id, string name)
            : base(id, name, PubRecordProperty.Class)
        {
        }

        private ECFRecord(int id, List<string> names, Dictionary<PubRecordProperty, RecordData> propertyBag)
            : base(id, names, propertyBag)
        {
        }

        protected override PubRecord MakeCopy(List<string> names, Dictionary<PubRecordProperty, RecordData> propertyBag)
        {
            return new ECFRecord(ID, new List<string>(names), new Dictionary<PubRecordProperty, RecordData>(propertyBag));
        }
    }
}
