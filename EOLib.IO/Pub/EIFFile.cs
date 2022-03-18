using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public class EIFFile : BasePubFile<EIFRecord>
    {
        public override string FileType => "EIF";

        public EIFFile()
        {
        }

        public EIFFile(int checksum, List<EIFRecord> data)
            : base (checksum, data)
        {
        }

        protected override BasePubFile<EIFRecord> MakeCopy()
        {
            return new EIFFile(CheckSum, new List<EIFRecord>(this));
        }
    }
}
