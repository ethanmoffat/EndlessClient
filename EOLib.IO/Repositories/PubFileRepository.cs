// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.IO.Pub;

namespace EOLib.IO.Repositories
{
    public class PubFileRepository : IPubFileRepository, IPubFileProvider
    {
        public IPubFile<EIFRecord> EIFFile { get; set; }

        public IPubFile<ENFRecord> ENFFile { get; set; }

        public IPubFile<ESFRecord> ESFFile { get; set; }

        public IPubFile<ECFRecord> ECFFile { get; set; }
    }
}