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
    public class ESFRecord : IPubRecord
    {
        public const int DATA_SIZE = 51;

        public int RecordSize { get { return DATA_SIZE; } }

        public int NameCount { get { return 2; } }

        public int ID { get; set; }

        public string Name { get; set; }
        public string Shout { get; set; }

        public short Icon { get; set; }
        public short Graphic { get; set; }

        public short TP { get; set; }
        public short SP { get; set; }

        public byte CastTime { get; set; }

        public SpellType Type { get; set; }
        public SpellTargetRestrict TargetRestrict { get; set; }
        public SpellTarget Target { get; set; }

        public short MinDam { get; set; }
        public short MaxDam { get; set; }
        public short Accuracy { get; set; }
        public short HP { get; set; }

        public TValue Get<TValue>(PubRecordPropertyType type)
        {
            var name = Enum.GetName(type.GetType(), type) ?? "";
            if (!name.StartsWith("Global") && !name.StartsWith("Spell"))
                throw new ArgumentOutOfRangeException("type", "Unsupported property requested for ESFRecord");

            if (name.StartsWith("Global"))
                name = name.Substring(6);
            else if (name.StartsWith("Spell"))
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
                mem.WriteByte(numberEncoderService.EncodeNumber(Shout.Length, 1)[0]);
                byte[] name = Encoding.ASCII.GetBytes(Name);
                byte[] shout = Encoding.ASCII.GetBytes(Shout); //shout, shout, let it all out!
                mem.Write(name, 0, name.Length);
                mem.Write(shout, 0, shout.Length);

                mem.Write(numberEncoderService.EncodeNumber(Icon, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Graphic, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(TP, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(SP, 2), 0, 2);
                mem.WriteByte(numberEncoderService.EncodeNumber(CastTime, 1)[0]);

                mem.Seek(11, SeekOrigin.Begin);
                mem.WriteByte(numberEncoderService.EncodeNumber((byte)Type, 1)[0]);

                mem.Seek(17, SeekOrigin.Begin);
                mem.WriteByte(numberEncoderService.EncodeNumber((byte)TargetRestrict, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber((byte)Target, 1)[0]);

                mem.Seek(23, SeekOrigin.Begin);
                mem.Write(numberEncoderService.EncodeNumber(MinDam, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(MaxDam, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Accuracy, 2), 0, 2);

                mem.Seek(34, SeekOrigin.Begin);
                mem.Write(numberEncoderService.EncodeNumber(HP, 2), 0, 2);
            }

            return ret;
        }

        public void SetNames(string[] names)
        {
            if (names.Length != NameCount)
                throw new ArgumentOutOfRangeException("names", "Incorrect number of names, must be " + NameCount);

            Name = names[0];
            Shout = names[1];
        }

        public void DeserializeFromByteArray(byte[] recordBytes, INumberEncoderService numberEncoderService)
        {
            Icon = (short)numberEncoderService.DecodeNumber(recordBytes[0], recordBytes[1]);
            Graphic = (short)numberEncoderService.DecodeNumber(recordBytes[2], recordBytes[3]);
            TP = (short)numberEncoderService.DecodeNumber(recordBytes[4], recordBytes[5]);
            SP = (short)numberEncoderService.DecodeNumber(recordBytes[6], recordBytes[7]);
            CastTime = (byte)numberEncoderService.DecodeNumber(recordBytes[8]);

            Type = (SpellType)numberEncoderService.DecodeNumber(recordBytes[11]);
            TargetRestrict = (SpellTargetRestrict)numberEncoderService.DecodeNumber(recordBytes[17]);
            Target = (SpellTarget)numberEncoderService.DecodeNumber(recordBytes[18]);

            MinDam = (short)numberEncoderService.DecodeNumber(recordBytes[23], recordBytes[24]);
            MaxDam = (short)numberEncoderService.DecodeNumber(recordBytes[25], recordBytes[26]);
            Accuracy = (short)numberEncoderService.DecodeNumber(recordBytes[27], recordBytes[28]);
            HP = (short)numberEncoderService.DecodeNumber(recordBytes[34], recordBytes[35]);
        }
    }
}
