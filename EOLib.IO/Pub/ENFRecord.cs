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

        public byte UnkByte2 { get; set; }

        public short Boss { get; set; }
        public short Child { get; set; }
        public NPCType Type { get; set; }

        public short UnkShort14 { get; set; }

        public short VendorID { get; set; }

        public int HP { get; set; }

        public short MinDam { get; set; }
        public short MaxDam { get; set; }

        public short Accuracy { get; set; }
        public short Evade { get; set; }
        public short Armor { get; set; }

        public byte UnkByte26 { get; set; }
        public short UnkShort27 { get; set; }
        public short UnkShort29 { get; set; }

        public short ElementWeak { get; set; }
        public short ElementWeakPower { get; set; }

        public byte UnkByte35 { get; set; }

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

                mem.WriteByte(numberEncoderService.EncodeNumber(UnkByte2, 1)[0]);

                mem.Write(numberEncoderService.EncodeNumber(Boss, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Child, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber((short)Type, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(VendorID, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(HP, 3), 0, 3);

                mem.Write(numberEncoderService.EncodeNumber(UnkShort14, 2), 0, 2);

                mem.Write(numberEncoderService.EncodeNumber(MinDam, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(MaxDam, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Accuracy, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Evade, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Armor, 2), 0, 2);

                mem.WriteByte(numberEncoderService.EncodeNumber(UnkByte26, 1)[0]);
                mem.Write(numberEncoderService.EncodeNumber(UnkShort27, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(UnkShort29, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(ElementWeak, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(ElementWeakPower, 2), 0, 2);
                mem.WriteByte(numberEncoderService.EncodeNumber(UnkByte35, 1)[0]);

                mem.Write(numberEncoderService.EncodeNumber(Exp, 3), 0, 3);
            }

            return ret;
        }

        public void DeserializeFromByteArray(byte[] recordBytes, INumberEncoderService numberEncoderService)
        {
            if (recordBytes.Length != DATA_SIZE)
                throw new ArgumentOutOfRangeException(nameof(recordBytes), "Data is not properly sized for correct deserialization");

            Graphic = numberEncoderService.DecodeNumber(recordBytes[0], recordBytes[1]);

            UnkByte2 = (byte)numberEncoderService.DecodeNumber(recordBytes[2]);

            Boss = (short)numberEncoderService.DecodeNumber(recordBytes[3], recordBytes[4]);
            Child = (short)numberEncoderService.DecodeNumber(recordBytes[5], recordBytes[6]);
            Type = (NPCType)numberEncoderService.DecodeNumber(recordBytes[7], recordBytes[8]);
            VendorID = (short)numberEncoderService.DecodeNumber(recordBytes[9], recordBytes[10]);
            HP = numberEncoderService.DecodeNumber(recordBytes[11], recordBytes[12], recordBytes[13]);

            UnkShort14 = (short)numberEncoderService.DecodeNumber(recordBytes[14], recordBytes[15]);

            MinDam = (short)numberEncoderService.DecodeNumber(recordBytes[16], recordBytes[17]);
            MaxDam = (short)numberEncoderService.DecodeNumber(recordBytes[18], recordBytes[19]);

            Accuracy = (short)numberEncoderService.DecodeNumber(recordBytes[20], recordBytes[21]);
            Evade = (short)numberEncoderService.DecodeNumber(recordBytes[22], recordBytes[23]);
            Armor = (short)numberEncoderService.DecodeNumber(recordBytes[24], recordBytes[25]);

            UnkByte26 = (byte)numberEncoderService.DecodeNumber(recordBytes[26]);
            UnkShort27 = (short)numberEncoderService.DecodeNumber(recordBytes[27], recordBytes[28]);
            UnkShort29 = (short)numberEncoderService.DecodeNumber(recordBytes[29], recordBytes[30]);
            ElementWeak = (short)numberEncoderService.DecodeNumber(recordBytes[31], recordBytes[32]);
            ElementWeakPower = (short)numberEncoderService.DecodeNumber(recordBytes[33], recordBytes[34]);
            UnkByte35 = (byte)numberEncoderService.DecodeNumber(recordBytes[35]);

            Exp = numberEncoderService.DecodeNumber(recordBytes[36], recordBytes[37], recordBytes[38]);
        }
    }
}