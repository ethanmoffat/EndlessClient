// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Old
{
    internal class ItemRecordFactory : IDataRecordFactory<ItemRecord>
    {
        public int RecordSizeInBytes { get { return ItemRecord.DATA_SIZE; } }

        public ItemRecord CreateRecord(int id)
        {
            return new ItemRecord(id);
        }
    }
}
