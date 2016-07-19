// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EOLib.Domain;
using EOLib.Net;

namespace EOLib.IO
{
    public class ItemRecord : IDataRecord
    {
        public const int DATA_SIZE = 58;

        public int ID { get; set; }

        public int NameCount { get { return 1; } }
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

        //Three item-specific parameters
        //eoserv uses unions, but they can just point to the same private data member
        private int itemSpecificParam1;
        private byte itemSpecificParam2;
        private byte itemSpecificParam3;

        public int ScrollMap { get { return itemSpecificParam1; } set { itemSpecificParam1 = value; } }
        public int DollGraphic { get { return itemSpecificParam1; } set { itemSpecificParam1 = value; } }
        public int ExpReward { get { return itemSpecificParam1; } set { itemSpecificParam1 = value; } }
        public int HairColor { get { return itemSpecificParam1; } set { itemSpecificParam1 = value; } }
        public int Effect { get { return itemSpecificParam1; } set { itemSpecificParam1 = value; } }
        public int Key { get { return itemSpecificParam1; } set { itemSpecificParam1 = value; } }

        /// <summary>
        /// 0 == female and 1 == male
        /// </summary>
        public byte Gender { get { return itemSpecificParam2; } set { itemSpecificParam2 = value; } }
        public byte ScrollX { get { return itemSpecificParam2; } set { itemSpecificParam2 = value; } }

        public byte ScrollY { get { return itemSpecificParam3; } set { itemSpecificParam3 = value; } }
        public byte DualWieldDollGraphic { get { return itemSpecificParam3; } set { itemSpecificParam3 = value; } }

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

        public ItemRecord(int id)
        {
            ID = id;
        }

        public override string ToString()
        {
            return ID + " : " + Name;
        }

        public void SetNames(params string[] names)
        {
            if(names.Length != NameCount)
                throw new ArgumentException("Error: item record has invalid number of names");

            Name = names[0];
        }

        public void DeserializeFromByteArray(int version, byte[] rawData)
        {
            //version 0/1 support: 
            // 0 : Original EO spec
            // 1 : Ethan's updates with the 2 added SubTypes for FaceMask and HatNoHair

            if (version == 0 || version == 1)
            {
                Graphic = (short) OldPacket.DecodeNumber(rawData[0], rawData[1]);
                Type = (ItemType) OldPacket.DecodeNumber(rawData[2]);
                SubType = (ItemSubType) OldPacket.DecodeNumber(rawData[3]);

                Special = (ItemSpecial) OldPacket.DecodeNumber(rawData[4]);
                HP = (short) OldPacket.DecodeNumber(rawData[5], rawData[6]);
                TP = (short) OldPacket.DecodeNumber(rawData[7], rawData[8]);
                MinDam = (short) OldPacket.DecodeNumber(rawData[9], rawData[10]);
                MaxDam = (short) OldPacket.DecodeNumber(rawData[11], rawData[12]);
                Accuracy = (short) OldPacket.DecodeNumber(rawData[13], rawData[14]);
                Evade = (short) OldPacket.DecodeNumber(rawData[15], rawData[16]);
                Armor = (short) OldPacket.DecodeNumber(rawData[17], rawData[18]);

                Str = (byte) OldPacket.DecodeNumber(rawData[20]);
                Int = (byte) OldPacket.DecodeNumber(rawData[21]);
                Wis = (byte) OldPacket.DecodeNumber(rawData[22]);
                Agi = (byte) OldPacket.DecodeNumber(rawData[23]);
                Con = (byte) OldPacket.DecodeNumber(rawData[24]);
                Cha = (byte) OldPacket.DecodeNumber(rawData[25]);

                Light = (byte) OldPacket.DecodeNumber(rawData[26]);
                Dark = (byte) OldPacket.DecodeNumber(rawData[27]);
                Earth = (byte) OldPacket.DecodeNumber(rawData[28]);
                Air = (byte) OldPacket.DecodeNumber(rawData[29]);
                Water = (byte) OldPacket.DecodeNumber(rawData[30]);
                Fire = (byte) OldPacket.DecodeNumber(rawData[31]);

                ScrollMap = OldPacket.DecodeNumber(rawData[32], rawData[33], rawData[34]);
                ScrollX = (byte) OldPacket.DecodeNumber(rawData[35]);
                ScrollY = (byte) OldPacket.DecodeNumber(rawData[36]);

                LevelReq = (short) OldPacket.DecodeNumber(rawData[37], rawData[38]);
                ClassReq = (short) OldPacket.DecodeNumber(rawData[39], rawData[40]);

                StrReq = (short) OldPacket.DecodeNumber(rawData[41], rawData[42]);
                IntReq = (short) OldPacket.DecodeNumber(rawData[43], rawData[44]);
                WisReq = (short) OldPacket.DecodeNumber(rawData[45], rawData[46]);
                AgiReq = (short) OldPacket.DecodeNumber(rawData[47], rawData[48]);
                ConReq = (short) OldPacket.DecodeNumber(rawData[49], rawData[50]);
                ChaReq = (short) OldPacket.DecodeNumber(rawData[51], rawData[52]);

                Weight = (byte) OldPacket.DecodeNumber(rawData[55]);
                Size = (ItemSize) OldPacket.DecodeNumber(rawData[57]);

                if (ID == 365 && Name == "Gun")
                    SubType = ItemSubType.Ranged;
            }
            else
            {
                throw new FileLoadException("Unable to Load file with invalid version: " + version);
            }
        }

        public byte[] SerializeToByteArray()
        {
            byte[] ret = new byte[DATA_SIZE + 1 + Name.Length];
            for (int i = 0; i < ret.Length; ++i)
                ret[i] = 254;

            using (MemoryStream mem = new MemoryStream(ret))
            {
                mem.WriteByte(OldPacket.EncodeNumber(Name.Length, 1)[0]);
                byte[] name = Encoding.ASCII.GetBytes(Name);
                mem.Write(name, 0, name.Length);

                mem.Write(OldPacket.EncodeNumber(Graphic, 2), 0, 2);
                mem.WriteByte(OldPacket.EncodeNumber((byte)Type, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber((byte)SubType, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber((byte)Special, 1)[0]);

                mem.Write(OldPacket.EncodeNumber(HP, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(TP, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(MinDam, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(MaxDam, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(Accuracy, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(Evade, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(Armor, 2), 0, 2);

                mem.Seek(21 + Name.Length, SeekOrigin.Begin);
                mem.WriteByte(OldPacket.EncodeNumber(Str, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber(Int, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber(Wis, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber(Agi, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber(Con, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber(Cha, 1)[0]);

                mem.WriteByte(OldPacket.EncodeNumber(Light, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber(Dark, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber(Earth, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber(Air, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber(Water, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber(Fire, 1)[0]);

                mem.Write(OldPacket.EncodeNumber(ScrollMap, 3), 0, 3);
                mem.WriteByte(OldPacket.EncodeNumber(ScrollX, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber(ScrollY, 1)[0]);

                mem.Write(OldPacket.EncodeNumber(LevelReq, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(ClassReq, 2), 0, 2);

                mem.Write(OldPacket.EncodeNumber(StrReq, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(IntReq, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(WisReq, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(AgiReq, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(ConReq, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(ChaReq, 2), 0, 2);

                mem.Seek(56 + Name.Length, SeekOrigin.Begin);
                mem.WriteByte(OldPacket.EncodeNumber(Weight, 1)[0]);
                mem.Seek(58 + Name.Length, SeekOrigin.Begin);
                mem.WriteByte(OldPacket.EncodeNumber((byte)Size, 1)[0]);
            }

            return ret;
        }

        public EquipLocation GetEquipLocation()
        {
            switch (Type)
            {
                case ItemType.Accessory:
                    return EquipLocation.Accessory;
                case ItemType.Armlet:
                    return EquipLocation.Armlet1;
                case ItemType.Armor:
                    return EquipLocation.Armor;
                case ItemType.Belt:
                    return EquipLocation.Belt;
                case ItemType.Boots:
                    return EquipLocation.Boots;
                case ItemType.Bracer:
                    return EquipLocation.Bracer1;
                case ItemType.Gloves:
                    return EquipLocation.Gloves;
                case ItemType.Hat:
                    return EquipLocation.Hat;
                case ItemType.Necklace:
                    return EquipLocation.Necklace;
                case ItemType.Ring:
                    return EquipLocation.Ring1;
                case ItemType.Shield:
                    return EquipLocation.Shield;
                case ItemType.Weapon:
                    return EquipLocation.Weapon;
                default:
                    return EquipLocation.PAPERDOLL_MAX;
            }
        }
    }

    public class ItemFile : EODataFile<ItemRecord>
    {
        public ItemFile() : base(new ItemRecordFactory(), new NumberEncoderService()) { }

        public static ItemFile FromBytes(IEnumerable<byte> bytes)
        {
            var file = new ItemFile();
            using (var ms = new MemoryStream(bytes.ToArray()))
                file.LoadFromStream(ms);
            return file;
        }
    }
}
