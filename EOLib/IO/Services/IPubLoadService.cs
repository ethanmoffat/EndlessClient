// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Services
{
    public interface IPubLoadService<out T>
        where T : IDataRecord
    {
        IDataFile<T> LoadPubFromDefaultFile();

        IDataFile<T> LoadPubFromExplicitFile(string fileName);
    }
}
