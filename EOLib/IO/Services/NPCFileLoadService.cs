// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Services
{
    public class NPCFileLoadService : IPubLoadService<NPCRecord>
    {
        public IDataFile<NPCRecord> LoadPubFromDefaultFile()
        {
            return LoadPubFromExplicitFile(Constants.NPCFilePath);
        }

        public IDataFile<NPCRecord> LoadPubFromExplicitFile(string fileName)
        {
            var npcFile = new NPCFile();
            npcFile.Load(fileName);
            return npcFile;
        }
    }
}
