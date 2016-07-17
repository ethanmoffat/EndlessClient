// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EOLib.IO
{
    public interface IDataFile<out T>
        where T : IDataRecord
    {
        IReadOnlyList<T> Data { get; }
        int Version { get; }
        int Rid { get; }
        short Len { get; }

        void Load(string fileName);

        T GetRecordByID(int id);

        int GetIndexOfRecordByID(int id);
    }
}
