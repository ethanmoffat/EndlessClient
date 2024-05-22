using System.Collections.Generic;

namespace EOLib.IO.Pub
{
    public class ENFFile : BasePubFile<ENFRecord>
    {
        public override string FileType => "ENF";

        public ENFFile()
        {
        }

        public ENFFile(int id, IReadOnlyList<int> checksum, int totalLength, List<ENFRecord> data)
            : base(id, checksum, totalLength, data)
        {
        }

        protected override BasePubFile<ENFRecord> MakeCopy()
        {
            return new ENFFile(ID, CheckSum, TotalLength, new List<ENFRecord>(this));
        }
    }
}
