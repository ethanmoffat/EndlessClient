using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public class EIFFile : BasePubFile<EIFRecord>
    {
        public override string FileType => "EIF";

        public EIFFile()
        {
        }

        public EIFFile(int id, int checksum, int totalLength, List<EIFRecord> data)
            : base (id, checksum, totalLength, data)
        {
        }

        protected override BasePubFile<EIFRecord> MakeCopy()
        {
            return new EIFFile(ID, CheckSum, TotalLength, new List<EIFRecord>(this));
        }
    }
}
