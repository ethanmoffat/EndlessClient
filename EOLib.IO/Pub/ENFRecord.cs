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
    public class ENFRecord : IPubRecord
    {
        public const int DATA_SIZE = 39;

        public int RecordSize { get { return DATA_SIZE; } }

        public int NameCount { get { return 1; } }

        public int ID { get; set; }

        public string Name { get; set; }

        public int Graphic { get; set; }

        public short Boss { get; set; }
        public short Child { get; set; }
        public NPCType Type { get; set; }

        public short VendorID { get; set; }

        public int HP { get; set; }
        public ushort Exp { get; set; }
        public short MinDam { get; set; }
        public short MaxDam { get; set; }

        public short Accuracy { get; set; }
        public short Evade { get; set; }
        public short Armor { get; set; }

        public TValue Get<TValue>(PubRecordPropertyType type)
        {
            var name = Enum.GetName(type.GetType(), type) ?? "";
            if (!name.StartsWith("Global") && !name.StartsWith("NPC"))
                throw new ArgumentOutOfRangeException("type", "Unsupported property requested for ENFRecord");

            if (name.StartsWith("Global"))
                name = name.Substring(6);
            else if (name.StartsWith("NPC"))
                name = name.Substring(3);

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

                mem.Write(numberEncoderService.EncodeNumber(Graphic, 2), 0, 2);

                mem.Seek(3 + Name.Length, SeekOrigin.Begin);
                mem.Write(numberEncoderService.EncodeNumber(Boss, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Child, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber((short)Type, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(VendorID, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(HP, 3), 0, 3);

                mem.Seek(16 + Name.Length, SeekOrigin.Begin);
                mem.Write(numberEncoderService.EncodeNumber(MinDam, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(MaxDam, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Accuracy, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Evade, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Armor, 2), 0, 2);

                mem.Seek(36 + Name.Length, SeekOrigin.Begin);
                mem.Write(numberEncoderService.EncodeNumber(Exp, 2), 0, 2);
            }

            return ret;
        }

        public void SetNames(string[] names)
        {
            if (names.Length != NameCount)
                throw new ArgumentOutOfRangeException("names", "Incorrect number of names, must be " + NameCount);

            Name = names[0];
        }

        public void DeserializeFromByteArray(byte[] recordBytes, INumberEncoderService numberEncoderService)
        {
            Graphic = numberEncoderService.DecodeNumber(recordBytes[0], recordBytes[1]);
            Boss = (short)numberEncoderService.DecodeNumber(recordBytes[3], recordBytes[4]);
            Child = (short)numberEncoderService.DecodeNumber(recordBytes[5], recordBytes[6]);
            Type = (NPCType)numberEncoderService.DecodeNumber(recordBytes[7], recordBytes[8]);
            VendorID = (short)numberEncoderService.DecodeNumber(recordBytes[9], recordBytes[10]);
            HP = numberEncoderService.DecodeNumber(recordBytes[11], recordBytes[12], recordBytes[13]);
            MinDam = (short)numberEncoderService.DecodeNumber(recordBytes[16], recordBytes[17]);
            MaxDam = (short)numberEncoderService.DecodeNumber(recordBytes[18], recordBytes[19]);
            Accuracy = (short)numberEncoderService.DecodeNumber(recordBytes[20], recordBytes[21]);
            Evade = (short)numberEncoderService.DecodeNumber(recordBytes[22], recordBytes[23]);
            Armor = (short)numberEncoderService.DecodeNumber(recordBytes[24], recordBytes[25]);
            Exp = (ushort)numberEncoderService.DecodeNumber(recordBytes[36], recordBytes[37]);
        }
    }
}
