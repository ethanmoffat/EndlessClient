// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.IO;
using System.Text;
using EOLib.IO.Services;

namespace EOLib.IO.Pub
{
    public class ESFFile : BasePubFile<ESFRecord>
    {
        public override string FileType
        {
            get { return "ESF"; }
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

            var rawData = new byte[ESFRecord.DATA_SIZE];
            for (int i = 1; i <= recordsInFile && mem.Position < mem.Length; ++i)
            {
                var nameLength = numberEncoderService.DecodeNumber((byte)mem.ReadByte());
                var shoutLength = numberEncoderService.DecodeNumber((byte)mem.ReadByte());
                var rawName = new byte[nameLength];
                var rawShout = new byte[shoutLength];
                mem.Read(rawName, 0, nameLength);
                mem.Read(rawShout, 0, shoutLength);
                mem.Read(rawData, 0, ESFRecord.DATA_SIZE);

                var record = new ESFRecord
                {
                    ID = i,
                    Name = Encoding.ASCII.GetString(rawName),
                    Shout = Encoding.ASCII.GetString(rawShout)
                };
                record.DeserializeFromByteArray(rawData, numberEncoderService);

                _data.Add(record);
            }

            if (recordsInFile != Length)
                throw new IOException("Mismatch between expected length and actual length!");
        }
    }
}
