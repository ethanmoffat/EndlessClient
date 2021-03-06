﻿using System.IO;
using System.Text;
using EOLib.IO.Services;

namespace EOLib.IO.Pub
{
    public class ENFFile : BasePubFile<ENFRecord>
    {
        public override string FileType => "ENF";

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

            var rawData = new byte[ENFRecord.DATA_SIZE];
            for (int i = 1; i <= recordsInFile && mem.Position < mem.Length; ++i)
            {
                var nameLength = numberEncoderService.DecodeNumber((byte)mem.ReadByte());
                var rawName = new byte[nameLength];
                mem.Read(rawName, 0, nameLength);
                mem.Read(rawData, 0, ENFRecord.DATA_SIZE);

                var record = new ENFRecord { ID = i, Name = Encoding.ASCII.GetString(rawName) };
                record.DeserializeFromByteArray(rawData, numberEncoderService);

                _data.Add(record);
            }

            if (recordsInFile != Length)
                throw new IOException("Mismatch between expected length and actual length!");
        }
    }
}
