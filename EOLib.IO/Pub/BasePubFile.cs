using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.IO.Pub
{
    public abstract class BasePubFile<TRecord> : IPubFile<TRecord>
        where TRecord : class, IPubRecord, new()
    {
        private readonly List<TRecord> _data;

        /// <inheritdoc />
        public abstract string FileType { get; }

        /// <inheritdoc />
        public int ID { get; private set; }

        /// <inheritdoc />
        public IReadOnlyList<int> CheckSum { get; private set; }

        /// <inheritdoc />
        public int Length => _data.Count;

        /// <inheritdoc />
        public int TotalLength { get; private set; }

        /// <inheritdoc />
        public TRecord this[int id] => _data[id - 1];

        protected BasePubFile()
        {
            _data = new List<TRecord>();
        }

        protected BasePubFile(int id, IReadOnlyList<int> checksum, int totalLength, List<TRecord> data)
        {
            if (checksum.Count != 2)
                throw new ArgumentException("Checksum should be 2 eo 'short' values", nameof(checksum));

            ID = id;
            CheckSum = checksum;
            TotalLength = totalLength;
            _data = data;
        }

        public IPubFile WithID(int id)
        {
            var copy = MakeCopy();
            copy.ID = id;
            return copy;
        }

        /// <inheritdoc />
        public IPubFile WithCheckSum(IReadOnlyList<int> checksum)
        {
            if (checksum.Count != 2)
                throw new ArgumentException("Checksum should be 2 eo 'short' values", nameof(checksum));

            var copy = MakeCopy();
            copy.CheckSum = checksum;
            return copy;
        }

        /// <inheritdoc />
        public IPubFile WithTotalLength(int totalLength)
        {
            var copy = MakeCopy();
            copy.TotalLength = totalLength;
            return copy;
        }

        /// <inheritdoc />
        public IPubFile<TRecord> WithAddedRecord(TRecord record)
        {
            if (_data.Any(x => x.ID == record.ID))
                throw new ArgumentException($"A record with ID {record.ID} already exists in this pub file", nameof(record));

            if (record.ID != _data.Count + 1)
                throw new ArgumentException($"Record ID {record.ID} is beyond the bounds of the collection", nameof(record));

            var copy = MakeCopy();
            copy._data.Add(record);
            copy._data.Sort((a, b) => a.ID - b.ID);
            return copy;
        }

        /// <inheritdoc />
        public IPubFile<TRecord> WithInsertedRecord(TRecord record)
        {
            if (_data.Count < record.ID)
                throw new ArgumentException($"Record {record.ID} ({record.Name}) is not part of the pub file", nameof(record));

            var copy = MakeCopy();
            copy._data.Insert(record.ID - 1, record);
            AdjustIDs(copy._data);
            return copy;
        }

        /// <inheritdoc />
        public IPubFile<TRecord> WithUpdatedRecord(TRecord record)
        {
            if (_data.Count < record.ID)
                throw new ArgumentException($"Record {record.ID} ({record.Name}) is not part of the pub file", nameof(record));

            var copy = MakeCopy();
            copy._data[record.ID - 1] = record;
            return copy;
        }

        /// <inheritdoc />
        public IPubFile<TRecord> WithRemovedRecord(TRecord record)
        {
            if (_data.Count < record.ID)
                throw new ArgumentException($"Record {record.ID} ({record.Name}) is not part of the pub file", nameof(record));

            var copy = MakeCopy();
            copy._data.Remove(record);
            AdjustIDs(copy._data);
            return copy;
        }

        public IEnumerator<TRecord> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public object Clone()
        {
            return MakeCopy();
        }

        protected abstract BasePubFile<TRecord> MakeCopy();

        private static void AdjustIDs(List<TRecord> data)
        {
            for (int i = 0; i < data.Count; i++)
                data[i] = (TRecord)data[i].WithID(i + 1);
        }
    }
}