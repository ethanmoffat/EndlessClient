// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Old
{
    internal class NPCRecordFactory : IDataRecordFactory<NPCRecord>
    {
        public int RecordSizeInBytes { get { return NPCRecord.DATA_SIZE; } }

        public NPCRecord CreateRecord(int id)
        {
            return new NPCRecord(id);
        }
    }
}
