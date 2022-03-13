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

        public byte UnkA { get; set; }
        public byte UnkB { get; set; }

        public SpellType Type { get; set; }

        public byte UnkC { get; set; }
        public short UnkD { get; set; }

        public SpellTargetRestrict TargetRestrict { get; set; }
        public SpellTarget Target { get; set; }

        public byte UnkE { get; set; }
        public byte UnkF { get; set; }
        public short UnkG { get; set; }

        public short MinDam { get; set; }
        public short MaxDam { get; set; }
        public short Accuracy { get; set; }

        public short UnkH { get; set; }
        public short UnkI { get; set; }
        public byte UnkJ { get; set; }

        public short HP { get; set; }

        public short UnkK { get; set; }
        public byte UnkL { get; set; }
        public short UnkM { get; set; }
        public short UnkN { get; set; }
        public short UnkO { get; set; }
        public short UnkP { get; set; }
        public short UnkQ { get; set; }
        public short UnkR { get; set; }

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

                mem.WriteByte(numberEncoderService.EncodeNumber(UnkA, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(UnkB, 1)[0]);

                mem.Write(numberEncoderService.EncodeNumber((int)Type, 3), 0, 3); //This is documented as a 3 byte int.

                mem.WriteByte(numberEncoderService.EncodeNumber(UnkC, 1)[0]);
                mem.Write(numberEncoderService.EncodeNumber(UnkD, 2), 0, 2);

                mem.WriteByte(numberEncoderService.EncodeNumber((byte)TargetRestrict, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber((byte)Target, 1)[0]);

                mem.WriteByte(numberEncoderService.EncodeNumber(UnkE, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(UnkF, 1)[0]);
                mem.Write(numberEncoderService.EncodeNumber(UnkG, 2), 0, 2);

                mem.Write(numberEncoderService.EncodeNumber(MinDam, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(MaxDam, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Accuracy, 2), 0, 2);

                mem.Write(numberEncoderService.EncodeNumber(UnkH, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkI, 2), 0, 2);
                mem.WriteByte(numberEncoderService.EncodeNumber(UnkJ, 1)[0]);

                mem.Write(numberEncoderService.EncodeNumber(HP, 2), 0, 2);

                mem.Write(numberEncoderService.EncodeNumber(UnkK, 2), 0, 2);
                mem.WriteByte(numberEncoderService.EncodeNumber(UnkL, 1)[0]);
                mem.Write(numberEncoderService.EncodeNumber(UnkM, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkN, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkO, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkP, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkQ, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkR, 2), 0, 2);
            }

            return ret;
        }

        public void DeserializeFromByteArray(byte[] recordBytes, INumberEncoderService numberEncoderService)
        {
            if (recordBytes.Length != DATA_SIZE)
                throw new ArgumentOutOfRangeException(nameof(recordBytes), "Data is not properly sized for correct deserialization");

            Console.Write(this.Name + " [");
            for (int i = 0; i < recordBytes.Length; i++)
            {
                Console.Write(recordBytes[i] + "[pos:" + i + "], ");
            }
            Console.Write("]\n");

            Icon = (short)numberEncoderService.DecodeNumber(recordBytes[0], recordBytes[1]);
            Graphic = (short)numberEncoderService.DecodeNumber(recordBytes[2], recordBytes[3]);
            TP = (short)numberEncoderService.DecodeNumber(recordBytes[4], recordBytes[5]);
            SP = (short)numberEncoderService.DecodeNumber(recordBytes[6], recordBytes[7]);
            CastTime = (byte)numberEncoderService.DecodeNumber(recordBytes[8]);

            UnkA = (byte)numberEncoderService.DecodeNumber(recordBytes[9]);
            UnkB = (byte)numberEncoderService.DecodeNumber(recordBytes[10]);

            Type = (SpellType)numberEncoderService.DecodeNumber(recordBytes[11], recordBytes[12], recordBytes[13]);

            UnkC = (byte)numberEncoderService.DecodeNumber(recordBytes[14]);
            UnkD = (short)numberEncoderService.DecodeNumber(recordBytes[15], recordBytes[16]);

            TargetRestrict = (SpellTargetRestrict)numberEncoderService.DecodeNumber(recordBytes[17]);
            Target = (SpellTarget)numberEncoderService.DecodeNumber(recordBytes[18]);

            UnkE = (byte)numberEncoderService.DecodeNumber(recordBytes[19]);
            UnkF = (byte)numberEncoderService.DecodeNumber(recordBytes[20]);
            UnkG = (short)numberEncoderService.DecodeNumber(recordBytes[21], recordBytes[22]);

            MinDam = (short)numberEncoderService.DecodeNumber(recordBytes[23], recordBytes[24]);
            MaxDam = (short)numberEncoderService.DecodeNumber(recordBytes[25], recordBytes[26]);
            Accuracy = (short)numberEncoderService.DecodeNumber(recordBytes[27], recordBytes[28]);

            UnkH = (short)numberEncoderService.DecodeNumber(recordBytes[29], recordBytes[30]);
            UnkI = (short)numberEncoderService.DecodeNumber(recordBytes[31], recordBytes[32]);
            UnkJ = (byte)numberEncoderService.DecodeNumber(recordBytes[33]);

            HP = (short)numberEncoderService.DecodeNumber(recordBytes[34], recordBytes[35]);

            UnkK = (short)numberEncoderService.DecodeNumber(recordBytes[36], recordBytes[37]);
            UnkL = (byte)numberEncoderService.DecodeNumber(recordBytes[38]);
            UnkM = (short)numberEncoderService.DecodeNumber(recordBytes[39], recordBytes[40]);
            UnkN = (short)numberEncoderService.DecodeNumber(recordBytes[41], recordBytes[42]);
            UnkO = (short)numberEncoderService.DecodeNumber(recordBytes[43], recordBytes[44]);
            UnkP = (short)numberEncoderService.DecodeNumber(recordBytes[45], recordBytes[46]);
            UnkQ = (short)numberEncoderService.DecodeNumber(recordBytes[47], recordBytes[48]);
            UnkR = (short)numberEncoderService.DecodeNumber(recordBytes[49], recordBytes[50]);
        }
    }
}
