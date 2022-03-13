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

        public int RecordSize => DATA_SIZE;

        public int ID { get; set; }

        public string Name { get; set; }
        public string Shout { get; set; }

        public short Icon { get; set; }
        public short Graphic { get; set; }

        public short TP { get; set; }
        public short SP { get; set; }

        public byte CastTime { get; set; }

        public byte UnkByte9 { get; set; }
        public byte UnkByte10 { get; set; }

        public SpellType Type { get; set; }

        public byte UnkByte14 { get; set; }
        public short UnkShort15 { get; set; }

        public SpellTargetRestrict TargetRestrict { get; set; }
        public SpellTarget Target { get; set; }

        public byte UnkByte19 { get; set; }
        public byte UnkByte20 { get; set; }
        public short UnkShort21 { get; set; }

        public short MinDam { get; set; }
        public short MaxDam { get; set; }
        public short Accuracy { get; set; }

        public short UnkShort29 { get; set; }
        public short UnkShort31 { get; set; }
        public byte UnkByte33 { get; set; }

        public short HP { get; set; }

        public short UnkShort36 { get; set; }
        public byte UnkByte38 { get; set; }
        public short UnkShort39 { get; set; }
        public short UnkShort41 { get; set; }
        public short UnkShort43 { get; set; }
        public short UnkShort45 { get; set; }
        public short UnkShort47 { get; set; }
        public short UnkShort49 { get; set; }

        public TValue Get<TValue>(PubRecordProperty type)
        {
            var name = Enum.GetName(type.GetType(), type) ?? "";
            if (!name.StartsWith("Global") && !name.StartsWith("Spell"))
                throw new ArgumentOutOfRangeException(nameof(type), "Unsupported property requested for ESFRecord");

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
            var ret = Enumerable.Repeat<byte>(254, DATA_SIZE + 2 + Name.Length + Shout.Length).ToArray();

            using (var mem = new MemoryStream(ret))
            {
                mem.WriteByte(numberEncoderService.EncodeNumber(Name.Length, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(Shout.Length, 1)[0]);
                var name = Encoding.ASCII.GetBytes(Name);
                var shout = Encoding.ASCII.GetBytes(Shout); //shout, shout, let it all out!
                mem.Write(name, 0, name.Length);
                mem.Write(shout, 0, shout.Length);

                mem.Write(numberEncoderService.EncodeNumber(Icon, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Graphic, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(TP, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(SP, 2), 0, 2);
                mem.WriteByte(numberEncoderService.EncodeNumber(CastTime, 1)[0]);

                mem.WriteByte(numberEncoderService.EncodeNumber(UnkByte9, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(UnkByte10, 1)[0]);

                mem.Write(numberEncoderService.EncodeNumber((int)Type, 3), 0, 3); //This is documented as a 3 byte int.

                mem.WriteByte(numberEncoderService.EncodeNumber(UnkByte14, 1)[0]);
                mem.Write(numberEncoderService.EncodeNumber(UnkShort15, 2), 0, 2);

                mem.WriteByte(numberEncoderService.EncodeNumber((byte)TargetRestrict, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber((byte)Target, 1)[0]);

                mem.WriteByte(numberEncoderService.EncodeNumber(UnkByte19, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(UnkByte20, 1)[0]);
                mem.Write(numberEncoderService.EncodeNumber(UnkShort21, 2), 0, 2);

                mem.Write(numberEncoderService.EncodeNumber(MinDam, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(MaxDam, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Accuracy, 2), 0, 2);

                mem.Write(numberEncoderService.EncodeNumber(UnkShort29, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkShort31, 2), 0, 2);
                mem.WriteByte(numberEncoderService.EncodeNumber(UnkByte33, 1)[0]);

                mem.Write(numberEncoderService.EncodeNumber(HP, 2), 0, 2);

                mem.Write(numberEncoderService.EncodeNumber(UnkShort36, 2), 0, 2);
                mem.WriteByte(numberEncoderService.EncodeNumber(UnkByte38, 1)[0]);
                mem.Write(numberEncoderService.EncodeNumber(UnkShort39, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkShort41, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkShort43, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkShort45, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkShort47, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkShort49, 2), 0, 2);
            }

            return ret;
        }

        public void DeserializeFromByteArray(byte[] recordBytes, INumberEncoderService numberEncoderService)
        {
            if (recordBytes.Length != DATA_SIZE)
                throw new ArgumentOutOfRangeException(nameof(recordBytes), "Data is not properly sized for correct deserialization");

            Icon = (short)numberEncoderService.DecodeNumber(recordBytes[0], recordBytes[1]);
            Graphic = (short)numberEncoderService.DecodeNumber(recordBytes[2], recordBytes[3]);
            TP = (short)numberEncoderService.DecodeNumber(recordBytes[4], recordBytes[5]);
            SP = (short)numberEncoderService.DecodeNumber(recordBytes[6], recordBytes[7]);
            CastTime = (byte)numberEncoderService.DecodeNumber(recordBytes[8]);

            UnkByte9 = (byte)numberEncoderService.DecodeNumber(recordBytes[9]);
            UnkByte10 = (byte)numberEncoderService.DecodeNumber(recordBytes[10]);

            Type = (SpellType)numberEncoderService.DecodeNumber(recordBytes[11], recordBytes[12], recordBytes[13]);

            UnkByte14 = (byte)numberEncoderService.DecodeNumber(recordBytes[14]);
            UnkShort15 = (short)numberEncoderService.DecodeNumber(recordBytes[15], recordBytes[16]);

            TargetRestrict = (SpellTargetRestrict)numberEncoderService.DecodeNumber(recordBytes[17]);
            Target = (SpellTarget)numberEncoderService.DecodeNumber(recordBytes[18]);

            UnkByte19 = (byte)numberEncoderService.DecodeNumber(recordBytes[19]);
            UnkByte20 = (byte)numberEncoderService.DecodeNumber(recordBytes[20]);
            UnkShort21 = (short)numberEncoderService.DecodeNumber(recordBytes[21], recordBytes[22]);

            MinDam = (short)numberEncoderService.DecodeNumber(recordBytes[23], recordBytes[24]);
            MaxDam = (short)numberEncoderService.DecodeNumber(recordBytes[25], recordBytes[26]);
            Accuracy = (short)numberEncoderService.DecodeNumber(recordBytes[27], recordBytes[28]);

            UnkShort29 = (short)numberEncoderService.DecodeNumber(recordBytes[29], recordBytes[30]);
            UnkShort31 = (short)numberEncoderService.DecodeNumber(recordBytes[31], recordBytes[32]);
            UnkByte33 = (byte)numberEncoderService.DecodeNumber(recordBytes[33]);

            HP = (short)numberEncoderService.DecodeNumber(recordBytes[34], recordBytes[35]);

            UnkShort36 = (short)numberEncoderService.DecodeNumber(recordBytes[36], recordBytes[37]);
            UnkByte38 = (byte)numberEncoderService.DecodeNumber(recordBytes[38]);
            UnkShort39 = (short)numberEncoderService.DecodeNumber(recordBytes[39], recordBytes[40]);
            UnkShort41 = (short)numberEncoderService.DecodeNumber(recordBytes[41], recordBytes[42]);
            UnkShort43 = (short)numberEncoderService.DecodeNumber(recordBytes[43], recordBytes[44]);
            UnkShort45 = (short)numberEncoderService.DecodeNumber(recordBytes[45], recordBytes[46]);
            UnkShort47 = (short)numberEncoderService.DecodeNumber(recordBytes[47], recordBytes[48]);
            UnkShort49 = (short)numberEncoderService.DecodeNumber(recordBytes[49], recordBytes[50]);
        }
    }
}