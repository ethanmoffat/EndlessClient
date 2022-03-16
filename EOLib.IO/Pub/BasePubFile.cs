using System;
using System.Collections;
using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public abstract class BasePubFile<TRecord> : IPubFile<TRecord>
        where TRecord : class, IPubRecord, new()
    {
        private readonly List<TRecord> _data;

        public abstract string FileType { get; }

        public int CheckSum { get; private set; }

        public int Length => _data.Count;

        // pub files use 1-based indexing
        public TRecord this[int id] => _data[id - 1];

        protected BasePubFile()
        {
            _data = new List<TRecord>();
        }

        protected BasePubFile(int checksum, List<TRecord> data)
        {
            CheckSum = checksum;
            _data = data;
        }

        public IPubFile WithCheckSum(int checksum)
        {
            var copy = MakeCopy();
            copy.CheckSum = checksum;
            return copy;
        }

        public IPubFile<TRecord> WithAddedRecord(TRecord record)
        {
            var copy = MakeCopy();
            copy._data.Add(record);
            return copy;
        }

        public IPubFile<TRecord> WithUpdatedRecord(TRecord record)
        {
            if (_data.Count <= record.ID)
                throw new ArgumentException($"Record {record.ID} ({record.Name}) is not part of the pub file");

            var copy = MakeCopy();
            copy._data[record.ID] = record;
            return copy;
        }

        public IPubFile<TRecord> WithRemovedRecord(TRecord record)
        {
            var copy = MakeCopy();
            if (copy._data.Remove(record))
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

        protected abstract BasePubFile<TRecord> MakeCopy();

        private static void AdjustIDs(List<TRecord> data)
        {
            for (int i = 0; i < data.Count; i++)
                data[i] = (TRecord)data[i].WithID(i);
        }
    }
}
