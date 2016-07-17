// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.IO.Repositories
{
    public class PubFileRepository : IPubFileRepository, IPubFileProvider
    {
        public IDataFile<ItemRecord> ItemFile { get; set; }

        public IDataFile<NPCRecord> NPCFile { get; set; }

        public IDataFile<SpellRecord> SpellFile { get; set; }

        public IDataFile<ClassRecord> ClassFile { get; set; }
    }
}