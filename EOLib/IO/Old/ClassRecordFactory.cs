// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Old
{
    internal class ClassRecordFactory : IDataRecordFactory<ClassRecord>
    {
        public int RecordSizeInBytes { get { return ClassRecord.DATA_SIZE; } }

        public ClassRecord CreateRecord(int id)
        {
            return new ClassRecord(id);
        }
    }
}
