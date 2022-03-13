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

        public int RecordSize => DATA_SIZE;

        public int ID { get; set; }

        public string Name { get; set; }

        public int Graphic { get; set; }

        public byte UnkA { get; set; }

        public short Boss { get; set; }
        public short Child { get; set; }
        public NPCType Type { get; set; }

        public short UnkB { get; set; }

        public short VendorID { get; set; }

        public int HP { get; set; }

        public short MinDam { get; set; }
        public short MaxDam { get; set; }

        public short Accuracy { get; set; }
        public short Evade { get; set; }
        public short Armor { get; set; }

        public byte UnkC { get; set; }
        public short UnkD { get; set; }
        public short UnkE { get; set; }
        public short UnkF { get; set; }
        public short UnkG { get; set; }
        public byte UnkH { get; set; }

        public int Exp { get; set; }

        public TValue Get<TValue>(PubRecordProperty type)
        {
            var name = Enum.GetName(type.GetType(), type) ?? "";
            if (!name.StartsWith("Global") && !name.StartsWith("NPC"))
                throw new ArgumentOutOfRangeException(nameof(type), "Unsupported property requested for ENFRecord");

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

                mem.WriteByte(numberEncoderService.EncodeNumber(UnkA, 1)[0]);

                mem.Write(numberEncoderService.EncodeNumber(Boss, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Child, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber((short)Type, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(VendorID, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(HP, 3), 0, 3);

                mem.Write(numberEncoderService.EncodeNumber(UnkB, 2), 0, 2);

                mem.Write(numberEncoderService.EncodeNumber(MinDam, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(MaxDam, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Accuracy, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Evade, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Armor, 2), 0, 2);

                mem.WriteByte(numberEncoderService.EncodeNumber(UnkC, 1)[0]);
                mem.Write(numberEncoderService.EncodeNumber(UnkD, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkE, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkF, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkG, 2), 0, 2);
                mem.WriteByte(numberEncoderService.EncodeNumber(UnkH, 1)[0]);

                mem.Write(numberEncoderService.EncodeNumber(Exp, 3), 0, 3);
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

            Graphic = numberEncoderService.DecodeNumber(recordBytes[0], recordBytes[1]);

            UnkA = (byte)numberEncoderService.DecodeNumber(recordBytes[2]);

            Boss = (short)numberEncoderService.DecodeNumber(recordBytes[3], recordBytes[4]);
            Child = (short)numberEncoderService.DecodeNumber(recordBytes[5], recordBytes[6]);
            Type = (NPCType)numberEncoderService.DecodeNumber(recordBytes[7], recordBytes[8]);
            VendorID = (short)numberEncoderService.DecodeNumber(recordBytes[9], recordBytes[10]);
            HP = numberEncoderService.DecodeNumber(recordBytes[11], recordBytes[12], recordBytes[13]);

            UnkB = (short)numberEncoderService.DecodeNumber(recordBytes[14], recordBytes[15]);

            MinDam = (short)numberEncoderService.DecodeNumber(recordBytes[16], recordBytes[17]);
            MaxDam = (short)numberEncoderService.DecodeNumber(recordBytes[18], recordBytes[19]);

            Accuracy = (short)numberEncoderService.DecodeNumber(recordBytes[20], recordBytes[21]);
            Evade = (short)numberEncoderService.DecodeNumber(recordBytes[22], recordBytes[23]);
            Armor = (short)numberEncoderService.DecodeNumber(recordBytes[24], recordBytes[25]);

            UnkC = (byte)numberEncoderService.DecodeNumber(recordBytes[26]);
            UnkD = (short)numberEncoderService.DecodeNumber(recordBytes[27], recordBytes[28]);
            UnkE = (short)numberEncoderService.DecodeNumber(recordBytes[29], recordBytes[30]);
            UnkF = (short)numberEncoderService.DecodeNumber(recordBytes[31], recordBytes[32]);
            UnkG = (short)numberEncoderService.DecodeNumber(recordBytes[33], recordBytes[34]);
            UnkH = (byte)numberEncoderService.DecodeNumber(recordBytes[35]);

            Exp = numberEncoderService.DecodeNumber(recordBytes[36], recordBytes[37], recordBytes[38]);
        }
    }
}
