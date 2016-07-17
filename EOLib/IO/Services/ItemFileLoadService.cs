// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Services
{
    public class ItemFileLoadService : IPubLoadService<ItemRecord>
    {
        public IDataFile<ItemRecord> LoadPubFromDefaultFile()
        {
            return LoadPubFromExplicitFile(Constants.ItemFilePath);
        }

        public IDataFile<ItemRecord> LoadPubFromExplicitFile(string fileName)
        {
            var pubFile = new ItemFile();
            pubFile.Load(fileName);
            return pubFile;
        }
    }
}
