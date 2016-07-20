// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Diagnostics;
using System.IO;
using System.Text;
using EOLib.IO.Services;

namespace EOLib.IO.Pub
{
    public class ECFFile : BasePubFile<ECFRecord>
    {
        public override string FileType
        {
            get { return "ECF"; }
        }

        public override byte[] SerializeToByteArray(INumberEncoderService numberEncoderService)
        {
            throw new System.NotImplementedException();
        }

        public override void DeserializeFromByteArray(byte[] bytes, INumberEncoderService numberEncoderService)
        {
            using (var mem = new MemoryStream(bytes))
                ReadFromStream(mem, numberEncoderService);
        }

        private void ReadFromStream(Stream mem, INumberEncoderService numberEncoderService)
        {
            mem.Seek(3, SeekOrigin.Begin);

            var checksum = new byte[4];
            mem.Read(checksum, 0, 4);
            CheckSum = numberEncoderService.DecodeNumber(checksum);

            var lenBytes = new byte[2];
            mem.Read(lenBytes, 0, 2);
            var recordsInFile = (short)numberEncoderService.DecodeNumber(lenBytes);

            mem.Seek(1, SeekOrigin.Current);

            var rawData = new byte[ECFRecord.DATA_SIZE];
            for (int i = 0; i < recordsInFile; ++i)
            {
                var nameLength = mem.ReadByte();
                var rawName = new byte[nameLength];
                mem.Read(rawName, 0, nameLength);
                mem.Read(rawData, 0, ECFRecord.DATA_SIZE);

                var record = new ECFRecord { ID = i, Name = Encoding.ASCII.GetString(rawName) };
                record.DeserializeFromByteArray(rawData, numberEncoderService);

                if (mem.Position == mem.Length || record.Name.ToLower() == "eof")
                    break;

                _data.Add(record);
            }

            Debug.Assert(recordsInFile == Length, "Mismatch between expected length and actual length!");
        }
    }
}
