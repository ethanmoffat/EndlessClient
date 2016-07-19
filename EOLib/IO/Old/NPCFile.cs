// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.IO;
using System.Linq;
using EOLib.Domain;
using EOLib.IO.Services;

namespace EOLib.IO.Old
{
    public class NPCFile : EODataFile<NPCRecord>
    {
        public NPCFile() : base(new NPCRecordFactory(), new NumberEncoderService()) { }

        public static NPCFile FromBytes(IEnumerable<byte> bytes)
        {
            var file = new NPCFile();
            using (var ms = new MemoryStream(bytes.ToArray()))
                file.LoadFromStream(ms);
            return file;
        }
    }
}
