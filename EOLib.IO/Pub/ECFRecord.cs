// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using EOLib.IO.Services;

namespace EOLib.IO.Pub
{
    public class ECFRecord : IPubRecord, IReadOnlyPubRecord
    {
        public const int DATA_SIZE = 14;

        public int RecordSize { get { return DATA_SIZE; } }

        public int ID { get; set; }

        public string Name { get; set; }

        public byte Base { get; set; }
        public byte Type { get; set; }

        public short Str { get; set; }
        public short Int { get; set; }
        public short Wis { get; set; }
        public short Agi { get; set; }
        public short Con { get; set; }
        public short Cha { get; set; }

        public TValue Get<TValue>(PubRecordProperty type)
        {
            var name = Enum.GetName(type.GetType(), type) ?? "";
            if (!name.StartsWith("Global") && !name.StartsWith("Class"))
                throw new ArgumentOutOfRangeException("type", "Unsupported property requested for ECFRecord");

            if (name.StartsWith("Global"))
                name = name.Substring(6);
            else if (name.StartsWith("Class"))
                name = name.Substring(5);

            var propertyInfo = GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            var boxedValue = propertyInfo.GetValue(this);

            return (TValue)boxedValue;
        }

        public byte[] SerializeToByteArray(INumberEncoderService numberEncoderService)
        {
            var ret = Enumerable.Repeat<byte>(254, DATA_SIZE + 1 + Name.Length).ToArray();

            using (var mem = new MemoryStream(ret))
            {
                mem.WriteByte(numberEncoderService.EncodeNumber(Name.Length, 1)[0]);
                var name = Encoding.ASCII.GetBytes(Name);
                mem.Write(name, 0, name.Length);

                mem.WriteByte(numberEncoderService.EncodeNumber(Base, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(Type, 1)[0]);

                mem.Write(numberEncoderService.EncodeNumber(Str, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Int, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Wis, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Agi, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Con, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Cha, 2), 0, 2);
            }

            return ret;
        }

        public void DeserializeFromByteArray(byte[] recordBytes, INumberEncoderService numberEncoderService)
        {
            if (recordBytes.Length != DATA_SIZE)
                throw new ArgumentOutOfRangeException("recordBytes", "Data is not properly sized for correct deserialization");

            Base = (byte)numberEncoderService.DecodeNumber(recordBytes[0]);
            Type = (byte)numberEncoderService.DecodeNumber(recordBytes[1]);

            Str = (short)numberEncoderService.DecodeNumber(recordBytes[2], recordBytes[3]);
            Int = (short)numberEncoderService.DecodeNumber(recordBytes[4], recordBytes[5]);
            Wis = (short)numberEncoderService.DecodeNumber(recordBytes[6], recordBytes[7]);
            Agi = (short)numberEncoderService.DecodeNumber(recordBytes[8], recordBytes[9]);
            Con = (short)numberEncoderService.DecodeNumber(recordBytes[10], recordBytes[11]);
            Cha = (short)numberEncoderService.DecodeNumber(recordBytes[12], recordBytes[13]);
        }
    }
}
