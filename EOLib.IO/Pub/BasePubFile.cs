// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.IO;
using System.Text;
using EOLib.IO.Services;

namespace EOLib.IO.Pub
{
    public abstract class BasePubFile<T> : IPubFile<T>, IReadOnlyPubFile<T>
        where T : IPubRecord, new()
    {
        protected readonly List<T> _data;

        public abstract string FileType { get; }

        public int CheckSum { get; set; }

        public int Length { get { return _data.Count; } }

        public T this[int id]
        {
            get { return _data[id - 1]; }
            set { _data[id - 1] = value; }
        }

        protected BasePubFile()
        {
            _data = new List<T>();
        }

        public byte[] SerializeToByteArray(INumberEncoderService numberEncoderService)
        {
            using (var mem = new MemoryStream()) //write to memory so we can get a CRC for the new RID value
            {
                mem.Write(Encoding.ASCII.GetBytes(FileType), 0, 3);
                mem.Write(numberEncoderService.EncodeNumber(CheckSum, 4), 0, 4);
                mem.Write(numberEncoderService.EncodeNumber(Length + 1, 2), 0, 2);

                mem.WriteByte(1);

                for (int i = 1; i < _data.Count; ++i)
                {
                    byte[] toWrite = _data[i].SerializeToByteArray(numberEncoderService);
                    mem.Write(toWrite, 0, toWrite.Length);
                }

                var eofRecord = new T { ID = Length, Name = "EOF" }.SerializeToByteArray(numberEncoderService);
                mem.Write(eofRecord, 0, eofRecord.Length);

                return mem.ToArray();
            }
        }

        public abstract void DeserializeFromByteArray(byte[] bytes, INumberEncoderService numberEncoderService);
    }
}
