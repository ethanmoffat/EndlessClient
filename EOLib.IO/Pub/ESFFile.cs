using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public class ESFFile : BasePubFile<ESFRecord>
    {
        public override string FileType => "ESF";

        public ESFFile()
        {
        }

        public ESFFile(int checksum, List<ESFRecord> data)
            : base(checksum, data)
        {
        }

        protected override BasePubFile<ESFRecord> MakeCopy()
        {
            return new ESFFile(CheckSum, new List<ESFRecord>(this));
        }
    }
}
