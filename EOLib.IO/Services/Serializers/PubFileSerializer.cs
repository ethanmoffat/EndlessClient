using AutomaticTypeMapper;
using EOLib.IO.Pub;
using System;
using System.IO;
using System.Text;

namespace EOLib.IO.Services.Serializers
{
    [AutoMappedType]
    public class PubFileSerializer : IPubFileSerializer
    {
        private readonly INumberEncoderService _numberEncoderService;
        private readonly IPubRecordSerializer _pubRecordSerializer;

        public PubFileSerializer(INumberEncoderService numberEncoderService,
                                 IPubRecordSerializer pubRecordSerializer)
        {
            _numberEncoderService = numberEncoderService;
            _pubRecordSerializer = pubRecordSerializer;
        }

        public IPubFile<TRecord> DeserializeFromByteArray<TRecord>(int id, byte[] data, Func<IPubFile<TRecord>> fileFactory)
            where TRecord : class, IPubRecord, new()
        {
            using (var mem = new MemoryStream(data))
            {
                mem.Seek(3, SeekOrigin.Begin);

                var checksumBytes = new byte[4];
                mem.Read(checksumBytes, 0, 4);
                var checksum = _numberEncoderService.DecodeNumber(checksumBytes);

                var lenBytes = new byte[2];
                mem.Read(lenBytes, 0, 2);

                var file = (IPubFile<TRecord>)fileFactory()
                    .WithID(id)
                    .WithCheckSum(checksum)
                    .WithTotalLength(_numberEncoderService.DecodeNumber(lenBytes));

                mem.Seek(1, SeekOrigin.Current);

                var dummyRecord = new TRecord();
                var dataSize = dummyRecord.DataSize;
                var numberOfNames = dummyRecord.NumberOfNames;
                for (int i = 1; i <= file.TotalLength && mem.Position < mem.Length; ++i)
                {
                    int nameLength = 0;
                    for (int nameNdx = 0; nameNdx < numberOfNames; nameNdx++)
                        nameLength += _numberEncoderService.DecodeNumber((byte)mem.ReadByte());
                    mem.Seek(-numberOfNames, SeekOrigin.Current);

                    var rawData = new byte[nameLength + numberOfNames + dataSize];
                    mem.Read(rawData, 0, rawData.Length);

                    var record = _pubRecordSerializer.DeserializeFromByteArray(rawData, () => new TRecord().WithID(i));
                    file = file.WithAddedRecord((TRecord)record);
                }

                if (mem.Position < mem.Length)
                    throw new IOException($"Mismatch between expected length ({mem.Length}) and actual length ({mem.Position})!");

                return file;
            }
        }

        public byte[] SerializeToByteArray<TRecord>(IPubFile<TRecord> file, bool rewriteChecksum = true)
            where TRecord : class, IPubRecord, new()
        {
            byte[] fileBytes;

            using (var mem = new MemoryStream()) //write to memory so we can get a CRC for the new RID value
            {
                mem.Write(Encoding.ASCII.GetBytes(file.FileType), 0, 3);
                mem.Write(_numberEncoderService.EncodeNumber(0, 2), 0, 2);
                mem.Write(_numberEncoderService.EncodeNumber(0, 2), 0, 2);
                mem.Write(_numberEncoderService.EncodeNumber(file.Length, 2), 0, 2);

                mem.WriteByte(_numberEncoderService.EncodeNumber(1, 1)[0]);

                foreach (var dataRecord in file)
                {
                    var toWrite = _pubRecordSerializer.SerializeToByteArray(dataRecord);
                    mem.Write(toWrite, 0, toWrite.Length);
                }

                fileBytes = mem.ToArray();
            }

            var checksumBytes = _numberEncoderService.EncodeNumber(file.CheckSum, 4);
            if (rewriteChecksum)
            {
                var checksum = CRC32.Check(fileBytes);
                checksumBytes = _numberEncoderService.EncodeNumber((int)checksum, 4);
            }

            Array.Copy(checksumBytes, 0, fileBytes, 3, 4);
            return fileBytes;
        }
    }
}
