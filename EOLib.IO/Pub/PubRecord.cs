using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EOLib.IO.Pub
{
    public class PubRecord : IPubRecord
    {
        private readonly Lazy<int> _dataSize;

        private readonly List<string> _names;
        private readonly Dictionary<PubRecordProperty, RecordData> _propertyBag;

        public int ID { get; private set; }

        public string Name => _names.FirstOrDefault() ?? string.Empty;

        public IReadOnlyList<string> Names => _names;

        public virtual int NumberOfNames => 1;

        public int DataSize => _dataSize.Value;

        public IReadOnlyDictionary<PubRecordProperty, RecordData> Bag => _propertyBag;

        public PubRecord()
            : this(0, new List<string> { string.Empty }, new Dictionary<PubRecordProperty, RecordData>())
        {
        }

        public PubRecord(int id, string name, PubRecordProperty flag)
            : this(id, new List<string> { name }, flag)
        {
        }

        public PubRecord(int id, List<string> names, PubRecordProperty flag)
            : this(id, names, GetPropertiesWithFlag(flag))
        {
        }

        public PubRecord(int id, List<string> names, Dictionary<PubRecordProperty, RecordData> propertyBag)
        {
            ID = id;
            _names = names;
            _propertyBag = propertyBag;
            
            _dataSize = new Lazy<int>(() => _propertyBag.Values
                .GroupBy(x => x.Offset)
                .Select(x => x.First().Length)
                .Aggregate((a, b) => a + b));
        }

        public IPubRecord WithID(int id)
        {
            var copy = MakeCopy(_names, _propertyBag);
            copy.ID = id;
            return copy;
        }

        public IPubRecord WithNames(IReadOnlyList<string> names)
        {
            var copy = MakeCopy(_names, _propertyBag);
            copy._names.Clear();
            copy._names.AddRange(names);
            return copy;
        }

        public IPubRecord WithProperty(PubRecordProperty type, int value)
        {
            var copy = MakeCopy(_names, _propertyBag);
            var existing = copy._propertyBag[type];
            copy._propertyBag[type] = new RecordData(existing.Offset, existing.Length, value);
            return copy;
        }

        public T Get<T>(PubRecordProperty property) => CastTo<T>.From(Bag[property].Value);

        protected virtual PubRecord MakeCopy(List<string> names, Dictionary<PubRecordProperty, RecordData> propertyBag)
        {
            return new PubRecord(ID, new List<string>(names), new Dictionary<PubRecordProperty, RecordData>(propertyBag));
        }

        private static Dictionary<PubRecordProperty, RecordData> GetPropertiesWithFlag(PubRecordProperty flag)
        {
            var enumType = typeof(PubRecordProperty);
            var enumValues = ((PubRecordProperty[])enumType.GetEnumValues())
                .Where(x => x.HasFlag(flag))
                .ToList();

            return enumValues
                .Select(x => new { Key = x, Value = enumType.GetMember(x.ToString()) })
                .Select(x => new { x.Key, Value = x.Value.FirstOrDefault(m => m.DeclaringType == enumType) })
                .Select(x => new { x.Key, Value = x.Value.GetCustomAttributes<RecordDataAttribute>(false).SingleOrDefault() })
                .Where(x => x.Value != null)
                .ToDictionary(k => k.Key, v => new RecordData(v.Value.Offset, v.Value.Length, 0));
        }
    }
}
