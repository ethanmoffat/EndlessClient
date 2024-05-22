using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public class ECFFile : BasePubFile<ECFRecord>
    {
        public override string FileType => "ECF";

        public ECFFile()
        {
        }

        public ECFFile(int id, IReadOnlyList<int> checksum, int totalLength, List<ECFRecord> data)
            : base(id, checksum, totalLength, data)
        {
        }

        protected override BasePubFile<ECFRecord> MakeCopy()
        {
            return new ECFFile(ID, CheckSum, TotalLength, new List<ECFRecord>(this));
        }
    }
}
