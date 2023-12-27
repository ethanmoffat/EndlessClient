using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public class ESFFile : BasePubFile<ESFRecord>
    {
        public override string FileType => "ESF";

        public ESFFile()
        {
        }

        public ESFFile(int id, int checksum, int totalLength, List<ESFRecord> data)
            : base(id, checksum, totalLength, data)
        {
        }

        protected override BasePubFile<ESFRecord> MakeCopy()
        {
            return new ESFFile(ID, CheckSum, TotalLength, new List<ESFRecord>(this));
        }
    }
}
