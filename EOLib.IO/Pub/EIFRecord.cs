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
    public class EIFRecord : IPubRecord
    {
        public const int DATA_SIZE = 58;

        public int RecordSize => DATA_SIZE;

        public int ID { get; set; }

        public string Name { get; set; }

        public short Graphic { get; set; }
        public ItemType Type { get; set; }
        public ItemSubType SubType { get; set; }

        public ItemSpecial Special { get; set; }
        public short HP { get; set; }
        public short TP { get; set; }
        public short MinDam { get; set; }
        public short MaxDam { get; set; }
        public short Accuracy { get; set; }
        public short Evade { get; set; }
        public short Armor { get; set; }

        public byte Str { get; set; }
        public byte Int { get; set; }
        public byte Wis { get; set; }
        public byte Agi { get; set; }
        public byte Con { get; set; }
        public byte Cha { get; set; }

        public byte Light { get; set; }
        public byte Dark { get; set; }
        public byte Earth { get; set; }
        public byte Air { get; set; }
        public byte Water { get; set; }
        public byte Fire { get; set; }

        private int _itemSpecificParam1;
        private byte _itemSpecificParam2;
        private byte _itemSpecificParam3;

        public int ScrollMap { get { return _itemSpecificParam1; } set { _itemSpecificParam1 = value; } }
        public int DollGraphic { get { return _itemSpecificParam1; } set { _itemSpecificParam1 = value; } }
        public int ExpReward { get { return _itemSpecificParam1; } set { _itemSpecificParam1 = value; } }
        public int HairColor { get { return _itemSpecificParam1; } set { _itemSpecificParam1 = value; } }
        public int Effect { get { return _itemSpecificParam1; } set { _itemSpecificParam1 = value; } }
        public int Key { get { return _itemSpecificParam1; } set { _itemSpecificParam1 = value; } }

        public byte Gender { get { return _itemSpecificParam2; } set { _itemSpecificParam2 = value; } }
        public byte ScrollX { get { return _itemSpecificParam2; } set { _itemSpecificParam2 = value; } }

        public byte ScrollY { get { return _itemSpecificParam3; } set { _itemSpecificParam3 = value; } }
        public byte DualWieldDollGraphic { get { return _itemSpecificParam3; } set { _itemSpecificParam3 = value; } }

        public short LevelReq { get; set; }
        public short ClassReq { get; set; }
        public short StrReq { get; set; }
        public short IntReq { get; set; }
        public short WisReq { get; set; }
        public short AgiReq { get; set; }
        public short ConReq { get; set; }
        public short ChaReq { get; set; }

        public byte Weight { get; set; }

        public ItemSize Size { get; set; }

        public TValue Get<TValue>(PubRecordProperty type)
        {
            var name = Enum.GetName(type.GetType(), type) ?? "";
            if (!name.StartsWith("Global") && !name.StartsWith("Item"))
                throw new ArgumentOutOfRangeException(nameof(type), "Unsupported property requested for EIFRecord");

            if (name.StartsWith("Global"))
                name = name.Substring(6);
            else if (name.StartsWith("Item"))
                name = name.Substring(4);

            var propertyInfo = GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            var boxedValue = propertyInfo.GetValue(this);

            return (TValue) boxedValue;
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
                mem.WriteByte(numberEncoderService.EncodeNumber((byte) Type, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber((byte) SubType, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber((byte) Special, 1)[0]);

                mem.Write(numberEncoderService.EncodeNumber(HP, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(TP, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(MinDam, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(MaxDam, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Accuracy, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Evade, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(Armor, 2), 0, 2);

                mem.Seek(21 + Name.Length, SeekOrigin.Begin);
                mem.WriteByte(numberEncoderService.EncodeNumber(Str, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(Int, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(Wis, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(Agi, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(Con, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(Cha, 1)[0]);

                mem.WriteByte(numberEncoderService.EncodeNumber(Light, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(Dark, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(Earth, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(Air, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(Water, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(Fire, 1)[0]);

                mem.Write(numberEncoderService.EncodeNumber(ScrollMap, 3), 0, 3);
                mem.WriteByte(numberEncoderService.EncodeNumber(ScrollX, 1)[0]);
                mem.WriteByte(numberEncoderService.EncodeNumber(ScrollY, 1)[0]);

                mem.Write(numberEncoderService.EncodeNumber(LevelReq, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(ClassReq, 2), 0, 2);

                mem.Write(numberEncoderService.EncodeNumber(StrReq, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(IntReq, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(WisReq, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(AgiReq, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(ConReq, 2), 0, 2);
                mem.Write(numberEncoderService.EncodeNumber(ChaReq, 2), 0, 2);

                mem.Seek(56 + Name.Length, SeekOrigin.Begin);
                mem.WriteByte(numberEncoderService.EncodeNumber(Weight, 1)[0]);
                mem.Seek(58 + Name.Length, SeekOrigin.Begin);
                mem.WriteByte(numberEncoderService.EncodeNumber((byte) Size, 1)[0]);
            }

            return ret;
        }

        public void DeserializeFromByteArray(byte[] recordBytes, INumberEncoderService numberEncoderService)
        {
            if (recordBytes.Length != DATA_SIZE)
                throw new ArgumentOutOfRangeException(nameof(recordBytes), "Data is not properly sized for correct deserialization");

            Graphic = (short) numberEncoderService.DecodeNumber(recordBytes[0], recordBytes[1]);
            Type = (ItemType) numberEncoderService.DecodeNumber(recordBytes[2]);
            SubType = (ItemSubType) numberEncoderService.DecodeNumber(recordBytes[3]);

            Special = (ItemSpecial) numberEncoderService.DecodeNumber(recordBytes[4]);
            HP = (short) numberEncoderService.DecodeNumber(recordBytes[5], recordBytes[6]);
            TP = (short) numberEncoderService.DecodeNumber(recordBytes[7], recordBytes[8]);
            MinDam = (short) numberEncoderService.DecodeNumber(recordBytes[9], recordBytes[10]);
            MaxDam = (short) numberEncoderService.DecodeNumber(recordBytes[11], recordBytes[12]);
            Accuracy = (short) numberEncoderService.DecodeNumber(recordBytes[13], recordBytes[14]);
            Evade = (short) numberEncoderService.DecodeNumber(recordBytes[15], recordBytes[16]);
            Armor = (short) numberEncoderService.DecodeNumber(recordBytes[17], recordBytes[18]);

            Str = (byte) numberEncoderService.DecodeNumber(recordBytes[20]);
            Int = (byte) numberEncoderService.DecodeNumber(recordBytes[21]);
            Wis = (byte) numberEncoderService.DecodeNumber(recordBytes[22]);
            Agi = (byte) numberEncoderService.DecodeNumber(recordBytes[23]);
            Con = (byte) numberEncoderService.DecodeNumber(recordBytes[24]);
            Cha = (byte) numberEncoderService.DecodeNumber(recordBytes[25]);

            Light = (byte) numberEncoderService.DecodeNumber(recordBytes[26]);
            Dark = (byte) numberEncoderService.DecodeNumber(recordBytes[27]);
            Earth = (byte) numberEncoderService.DecodeNumber(recordBytes[28]);
            Air = (byte) numberEncoderService.DecodeNumber(recordBytes[29]);
            Water = (byte) numberEncoderService.DecodeNumber(recordBytes[30]);
            Fire = (byte) numberEncoderService.DecodeNumber(recordBytes[31]);

            ScrollMap = numberEncoderService.DecodeNumber(recordBytes[32], recordBytes[33], recordBytes[34]);
            ScrollX = (byte) numberEncoderService.DecodeNumber(recordBytes[35]);
            ScrollY = (byte) numberEncoderService.DecodeNumber(recordBytes[36]);

            LevelReq = (short) numberEncoderService.DecodeNumber(recordBytes[37], recordBytes[38]);
            ClassReq = (short) numberEncoderService.DecodeNumber(recordBytes[39], recordBytes[40]);

            StrReq = (short) numberEncoderService.DecodeNumber(recordBytes[41], recordBytes[42]);
            IntReq = (short) numberEncoderService.DecodeNumber(recordBytes[43], recordBytes[44]);
            WisReq = (short) numberEncoderService.DecodeNumber(recordBytes[45], recordBytes[46]);
            AgiReq = (short) numberEncoderService.DecodeNumber(recordBytes[47], recordBytes[48]);
            ConReq = (short) numberEncoderService.DecodeNumber(recordBytes[49], recordBytes[50]);
            ChaReq = (short) numberEncoderService.DecodeNumber(recordBytes[51], recordBytes[52]);

            Weight = (byte) numberEncoderService.DecodeNumber(recordBytes[55]);
            Size = (ItemSize) numberEncoderService.DecodeNumber(recordBytes[57]);

            if (ID == 365 && Name == "Gun")
                SubType = ItemSubType.Ranged;
        }
    }
}