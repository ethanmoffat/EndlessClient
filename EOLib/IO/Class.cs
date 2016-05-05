// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EOLib.Net;

namespace EOLib.IO
{
	public class ClassRecord : IDataRecord
	{
		public const int DATA_SIZE = 14;

		public int ID { get; set; }

		public int NameCount { get { return 1; } }
		public string Name { get; set; }

		public byte Base { get; set; }
		public byte Type { get; set; }

		public short Str { get; set; }
		public short Int { get; set; }
		public short Wis { get; set; }
		public short Agi { get; set; }
		public short Con { get; set; }
		public short Cha { get; set; }

		public ClassRecord(int id)
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
		}

		public void DeserializeFromByteArray(int version, byte[] rawData)
		{
			if (version != 0)
				throw new FileLoadException("Unable to Load file with invalid version: " + version);

			Base = (byte) OldPacket.DecodeNumber(rawData[0]);
			Type = (byte) OldPacket.DecodeNumber(rawData[1]);
			Str = (short) OldPacket.DecodeNumber(rawData[2], rawData[3]);
			Int = (short) OldPacket.DecodeNumber(rawData[4], rawData[5]);
			Wis = (short) OldPacket.DecodeNumber(rawData[6], rawData[7]);
			Agi = (short) OldPacket.DecodeNumber(rawData[8], rawData[9]);
			Con = (short) OldPacket.DecodeNumber(rawData[10], rawData[11]);
			Cha = (short) OldPacket.DecodeNumber(rawData[12], rawData[13]);
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

				mem.WriteByte(Base);
				mem.WriteByte(Type);

				mem.Write(OldPacket.EncodeNumber(Str, 2), 0, 2);
				mem.Write(OldPacket.EncodeNumber(Int, 2), 0, 2);
				mem.Write(OldPacket.EncodeNumber(Wis, 2), 0, 2);
				mem.Write(OldPacket.EncodeNumber(Agi, 2), 0, 2);
				mem.Write(OldPacket.EncodeNumber(Con, 2), 0, 2);
				mem.Write(OldPacket.EncodeNumber(Cha, 2), 0, 2);
			}

			return ret;
		}
	}

	public class ClassFile : EODataFile<ClassRecord>
	{
		public ClassFile() : base(new ClassRecordFactory()) { }

		public static ClassFile FromBytes(IEnumerable<byte> bytes)
		{
			var file = new ClassFile();
			using (var ms = new MemoryStream(bytes.ToArray()))
				file.LoadFromStream(ms);
			return file;
		}
	}
}
