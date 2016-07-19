// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Old;

namespace EOLib.IO.Services
{
    public class SpellFileLoadService : IPubLoadService<SpellRecord>
    {
        public IDataFile<SpellRecord> LoadPubFromDefaultFile()
        {
            return LoadPubFromExplicitFile(Constants.SpellFilePath);
        }

        public IDataFile<SpellRecord> LoadPubFromExplicitFile(string fileName)
        {
            var spellFile = new SpellFile();
            spellFile.Load(fileName);
            return spellFile;
        }
    }
}
