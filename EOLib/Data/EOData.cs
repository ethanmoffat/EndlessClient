using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EOLib.Data
{
	public interface IDataRecord
	{
		string Name { get; }

		/// <summary>
		/// The number of 'names' this data record has (usually 1)
		/// </summary>
		int NameCount { get; }

		/// <summary>
		/// Set the 'names' for this data record
		/// </summary>
		/// <param name="names">The names that were read from the file</param>
		void SetNames(params string[] names);

		byte[] SerializeToByteArray();

		void DeserializeFromByteArray(int version, byte[] rawData);

		// Require implementation of ToString() for debugging purposes
		// ReSharper disable once UnusedMember.Global
		string ToString();
	}

	public abstract class EODataFile
	{
		public List<IDataRecord> Data { get; protected set; }
		public int Version { get; protected set; }
		public int Rid { get; protected set; }
		public short Len { get; protected set; }
		
		protected string FilePath { private get; set; }

		private readonly IDataRecordFactory _factory;

		internal EODataFile(IDataRecordFactory factory)
		{
			_factory = factory;
		}

		protected abstract int GetDataSize();

		protected void Load(string fName)
		{
			using (FileStream sr = File.OpenRead(fName)) //throw exceptions on error
			{
				sr.Seek(3, SeekOrigin.Begin);

				byte[] rid = new byte[4];
				sr.Read(rid, 0, 4);
				Rid = Packet.DecodeNumber(rid);

				byte[] len = new byte[2];
				sr.Read(len, 0, 2);
				Len = (short) Packet.DecodeNumber(len);

				//indices are 1-based
				Data = new List<IDataRecord>(Len) {_factory.CreateRecord(0)};

				Version = Packet.DecodeNumber((byte) sr.ReadByte()); //this was originally seeked over

				byte[] rawData = new byte[GetDataSize()];

				for(int i = 1; i <= Len; ++i)
				{
					var record = _factory.CreateRecord(i);

					var nameLengths = new List<int>(2);
					for (int j = 0; j < record.NameCount; ++j)
					{
						var nameSize = Packet.DecodeNumber((byte) sr.ReadByte());
						nameLengths.Add(nameSize);
					}

					record.SetNames(nameLengths.Select(x =>
					{
						byte[] rawName = new byte[x];
						sr.Read(rawName, 0, x);
						return Encoding.ASCII.GetString(rawName);
					}).ToArray());

					sr.Read(rawData, 0, GetDataSize());
					record.DeserializeFromByteArray(Version, rawData);
					
					if (record.Name != "eof")
						Data.Add(record);
					if (sr.Read(new byte[1], 0, 1) != 1)
						break;
					sr.Seek(-1, SeekOrigin.Current);
				}
			}
		}

		///  <summary>
		///  Uses polymorphic features of the IDataType interface to save the different data types differently
		///  Headers for all types of files match, save for the first 3 bytes, which use the file extension to
		/// 		save the proper string.
		///  </summary>
		/// <param name="pubVersion">Version of the pub file to save. For Ethan's client, items should be 1, otherwise, this should be 0 (for now)</param>
		/// <param name="error">ref parameter that provides the Exception.Message string on an error condition</param>
		/// <returns>True if successful, false on failure. Use the 'error' parameter to check error message</returns>
		public bool Save(int pubVersion, out string error)
		{
			try
			{
				using (FileStream sw = File.Create(FilePath)) //throw exceptions on error
				{
					if (FilePath.Length <= 4 || !FilePath.Contains('.'))
						throw new ArgumentException("The filename of the data file must have a 3 letter extension. Use EIF, ENF, ESF, or ECF.");

					//get the extension to write as the first 3 bytes
					byte[] extension = Encoding.ASCII.GetBytes(FilePath.ToUpper().Substring(FilePath.LastIndexOf('.') + 1));
					if (extension.Length != 3)
						throw new ArgumentException("The filename of the data file must have a 3 letter extension. Use EIF, ENF, ESF, or ECF.");

					//allocate the data array for all the data to be saved
					byte[] allData;
					//write the file to memory first
					using (MemoryStream mem = new MemoryStream())
					{
						mem.Write(extension, 0, 3); //E[I|N|S|C]F at beginning
						mem.Write(Packet.EncodeNumber(Rid, 4), 0, 4); //rid
						mem.Write(Packet.EncodeNumber(Data.Count, 2), 0, 2); //len

						Version = pubVersion;
						mem.WriteByte(Packet.EncodeNumber(Version, 1)[0]); //new version check

						for (int i = 1; i < Data.Count; ++i)
						{
							byte[] toWrite = Data[i].SerializeToByteArray();
							mem.Write(toWrite, 0, toWrite.Length);
						}
						allData = mem.ToArray(); //get all data bytes
					}

					//write the data to the stream and overwrite whatever the rid is with the CRC
					CRC32 crc = new CRC32();
					uint newRid = crc.Check(allData, 7, (uint)allData.Length - 7);
					Rid = (int)newRid;
					sw.Write(allData, 0, allData.Length);
					sw.Seek(3, SeekOrigin.Begin); //skip first 3 bytes
					sw.Write(Packet.EncodeNumber(Rid, 4), 0, 4); //overwrite the 4 RID (revision ID) bytes
				}
			}
			catch (Exception ex)
			{
				error = ex.Message;
				return false;
			}

			error = "none";
			return true;
		}
	}
}
