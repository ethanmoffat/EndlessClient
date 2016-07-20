// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EOLib.Domain;
using EOLib.IO.Services;
using EOLib.Net;

namespace EOLib.IO.Old
{
    public class SpellRecord : IDataRecord
    {
        public const int DATA_SIZE = 51;

        public int ID { get; set; }

        public int NameCount { get { return 2; } }
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

        public SpellRecord(int id)
        {
            ID = id;
        }

        public override string ToString()
        {
            return ID + ": " + Name;
        }

        public void SetNames(params string[] names)
        {
            if (names.Length != NameCount)
                throw new ArgumentException("Error: item record has invalid number of names");

            Name = names[0];
            Shout = names[1];
        }

        public void DeserializeFromByteArray(int version, byte[] rawData)
        {
            if (version != 0)
                throw new FileLoadException("Unable to Load file with invalid version: " + version);
            Icon = (short) OldPacket.DecodeNumber(rawData[0], rawData[1]);
            Graphic = (short) OldPacket.DecodeNumber(rawData[2], rawData[3]);
            TP = (short) OldPacket.DecodeNumber(rawData[4], rawData[5]);
            SP = (short) OldPacket.DecodeNumber(rawData[6], rawData[7]);
            CastTime = (byte) OldPacket.DecodeNumber(rawData[8]);

            Type = (SpellType) OldPacket.DecodeNumber(rawData[11]);
            TargetRestrict = (SpellTargetRestrict) OldPacket.DecodeNumber(rawData[17]);
            Target = (SpellTarget) OldPacket.DecodeNumber(rawData[18]);

            MinDam = (short) OldPacket.DecodeNumber(rawData[23], rawData[24]);
            MaxDam = (short) OldPacket.DecodeNumber(rawData[25], rawData[26]);
            Accuracy = (short) OldPacket.DecodeNumber(rawData[27], rawData[28]);
            HP = (short) OldPacket.DecodeNumber(rawData[34], rawData[35]);
        }

        public byte[] SerializeToByteArray()
        {
            byte[] ret = new byte[DATA_SIZE + 1 + Name.Length];
            for (int i = 0; i < ret.Length; ++i)
                ret[i] = 254;

            using (MemoryStream mem = new MemoryStream(ret))
            {
                mem.WriteByte(OldPacket.EncodeNumber(Name.Length, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber(Shout.Length, 1)[0]);
                byte[] name = Encoding.ASCII.GetBytes(Name);
                byte[] shout = Encoding.ASCII.GetBytes(Shout); //shout, shout, let it all out!
                mem.Write(name, 0, name.Length);
                mem.Write(shout, 0, shout.Length);

                mem.Write(OldPacket.EncodeNumber(Icon, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(Graphic, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(TP, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(SP, 2), 0, 2);
                mem.WriteByte(OldPacket.EncodeNumber(CastTime, 1)[0]);

                mem.Seek(11, SeekOrigin.Begin);
                mem.WriteByte(OldPacket.EncodeNumber((byte)Type, 1)[0]);

                mem.Seek(17, SeekOrigin.Begin);
                mem.WriteByte(OldPacket.EncodeNumber((byte)TargetRestrict, 1)[0]);
                mem.WriteByte(OldPacket.EncodeNumber((byte)Target, 1)[0]);

                mem.Seek(23, SeekOrigin.Begin);
                mem.Write(OldPacket.EncodeNumber(MinDam, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(MaxDam, 2), 0, 2);
                mem.Write(OldPacket.EncodeNumber(Accuracy, 2), 0, 2);

                mem.Seek(34, SeekOrigin.Begin);
                mem.Write(OldPacket.EncodeNumber(HP, 2), 0, 2);
            }

            return ret;
        }
    }

    public class SpellFile : EODataFile<SpellRecord>
    {
        public SpellFile() : base(new SpellRecordFactory(), new NumberEncoderService()) { }

        public static SpellFile FromBytes(IEnumerable<byte> bytes)
        {
            var file = new SpellFile();
            using (var ms = new MemoryStream(bytes.ToArray()))
                file.LoadFromStream(ms);
            return file;
        }
    }
}
