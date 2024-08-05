using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public interface IPubRecord
    {
        int ID { get; }

        /// <summary>
        /// The first name in the list of names for the record
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Collection of all names in the record
        /// </summary>
        IReadOnlyList<string> Names { get; }

        /// <summary>
        /// Expected number of names per record in a data file (ESF files have 'name' and 'shout' variable strings)
        /// </summary>
        int NumberOfNames { get; }

        /// <summary>
        /// Constant size of a data record
        /// </summary>
        int DataSize { get; }

        IReadOnlyDictionary<PubRecordProperty, RecordData> Bag { get; }

        T Get<T>(PubRecordProperty property);

        IPubRecord WithID(int id);

        IPubRecord WithName(string name);

        IPubRecord WithNames(IReadOnlyList<string> name);

        IPubRecord WithProperty(PubRecordProperty type, int value);
    }
}
