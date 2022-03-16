using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public class ECFFile : BasePubFile<ECFRecord>
    {
        public override string FileType => "ECF";

        public ECFFile()
        {
        }

        public ECFFile(int checksum, List<ECFRecord> data)
            : base(checksum, data)
        {
        }

        protected override BasePubFile<ECFRecord> MakeCopy()
        {
            return new ECFFile(CheckSum, new List<ECFRecord>(this));
        }
    }
}
