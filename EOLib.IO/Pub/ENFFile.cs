using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public class ENFFile : BasePubFile<ENFRecord>
    {
        public override string FileType => "ENF";

        public ENFFile()
        {
        }

        public ENFFile(int checksum, List<ENFRecord> data)
            : base(checksum, data)
        {
        }

        protected override BasePubFile<ENFRecord> MakeCopy()
        {
            return new ENFFile(CheckSum, new List<ENFRecord>(this));
        }
    }
}
