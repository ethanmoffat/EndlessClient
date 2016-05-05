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
	public abstract class EODataFile<T> : IModifiableDataFile<T>
		where T : IDataRecord
	{
		public IReadOnlyList<T> Data { get; private set; }
		public int Version { get; private set; }
		public int Rid { get; private set; }
		public short Len { get; private set; }

		private readonly IDataRecordFactory<T> _factory;

		internal EODataFile(IDataRecordFactory<T> factory)
		{
			_factory = factory;
			Data = new List<T>();
		}

		public void Load(string fileName)
		{
			using (var sr = File.OpenRead(fileName)) //throw exceptions on error
			{
				sr.Seek(3, SeekOrigin.Begin);

				var rid = new byte[4];
				sr.Read(rid, 0, 4);
				Rid = OldPacket.DecodeNumber(rid);

				var len = new byte[2];
				sr.Read(len, 0, 2);
				Len = (short) OldPacket.DecodeNumber(len);

				//indices are 1-based
				var localData = new List<T>(Len) { _factory.CreateRecord(0) };

				Version = OldPacket.DecodeNumber((byte) sr.ReadByte()); //this was originally seeked over

				var rawData = new byte[_factory.RecordSizeInBytes];

				for(int i = 1; i <= Len; ++i)
				{
					var record = _factory.CreateRecord(i);

					var nameLengths = new List<int>(2);
					for (int j = 0; j < record.NameCount; ++j)
					{
						var nameSize = OldPacket.DecodeNumber((byte) sr.ReadByte());
						nameLengths.Add(nameSize);
					}

					record.SetNames(nameLengths.Select(x =>
					{
						var rawName = new byte[x];
						sr.Read(rawName, 0, x);
						return Encoding.ASCII.GetString(rawName);
					}).ToArray());

					sr.Read(rawData, 0, _factory.RecordSizeInBytes);
					record.DeserializeFromByteArray(Version, rawData);
					
					if (record.Name != "eof")
						localData.Add(record);
					if (sr.Read(new byte[1], 0, 1) != 1)
						break;
					sr.Seek(-1, SeekOrigin.Current);
				}

				Data = new List<T>(localData);
			}
		}

		public void Save(string fileName, int pubVersion = 0)
		{
			if (fileName.Length <= 4 || !fileName.Contains('.'))
				throw new ArgumentException(
					"The filename of the data file must have a 3 letter extension. Use EIF, ENF, ESF, or ECF.");

			//get the extension to write as the first 3 bytes
			byte[] extension = Encoding.ASCII.GetBytes(fileName.ToUpper().Substring(fileName.LastIndexOf('.') + 1));
			if (extension.Length != 3)
				throw new ArgumentException(
					"The filename of the data file must have a 3 letter extension. Use EIF, ENF, ESF, or ECF.");

			byte[] allData;
			using (var mem = new MemoryStream()) //write to memory so we can get a CRC for the new RID value
			{
				mem.Write(extension, 0, 3);
				mem.Write(OldPacket.EncodeNumber(Rid, 4), 0, 4);
				mem.Write(OldPacket.EncodeNumber(Data.Count, 2), 0, 2);

				Version = pubVersion;
				mem.WriteByte(OldPacket.EncodeNumber(Version, 1)[0]); //new version check

				for (int i = 1; i < Data.Count; ++i)
				{
					byte[] toWrite = Data[i].SerializeToByteArray();
					mem.Write(toWrite, 0, toWrite.Length);
				}
				allData = mem.ToArray();
			}

			using (var sw = File.Create(fileName))
			{
				var crc = new CRC32();
				var newRid = crc.Check(allData, 7, (uint) allData.Length - 7);
				Rid = (int) newRid;
				sw.Write(allData, 0, allData.Length);
				sw.Seek(3, SeekOrigin.Begin); //skip first 3 bytes
				sw.Write(OldPacket.EncodeNumber(Rid, 4), 0, 4); //overwrite the 4 RID (revision ID) bytes
			}
		}

		public T GetRecordByID(int id)
		{
			return Data.SingleOrDefault(x => x.ID == id);
		}

		public int GetIndexOfRecordByID(int id)
		{
			return Data.Select((record, ndx) => new Tuple<int, int>(record.ID, ndx))
					   .Single(x => x.Item1 == id)
					   .Item2;
		}

		public void ReplaceRecordAt(int index, T record)
		{
			var newList = Data.ToList();
			newList[index] = record;
			Data = newList;
		}
	}
}
