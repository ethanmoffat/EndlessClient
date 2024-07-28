using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public class EIFFile : BasePubFile<EIFRecord>
    {
        public override string FileType => "EIF";

        public EIFFile()
            : this(0, new List<int> { 0, 0 }, 0, new List<EIFRecord>())
        {
        }

        public EIFFile(int id, IReadOnlyList<int> checksum, int totalLength, List<EIFRecord> data)
            : base(id, checksum, totalLength, data)
        {
        }

        protected override BasePubFile<EIFRecord> MakeCopy()
        {
            return new EIFFile(ID, CheckSum, TotalLength, new List<EIFRecord>(this));
        }
    }
}