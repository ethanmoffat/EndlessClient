using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace EOLib.Data
{
	public enum EquipLocation
	{
		Boots = 0,
		Accessory,
		Gloves,
		Belt,
		Armor,
		Necklace,
		Hat,
		Shield,
		Weapon,
		Ring1,
		Ring2,
		Armlet1,
		Armlet2,
		Bracer1,
		Bracer2,
		PAPERDOLL_MAX = 15
	}

	public enum ItemType : byte
	{
		Static,
		UnknownType1,
		Money,
		Heal,
		Teleport,
		Spell,
		EXPReward,
		StatReward,
		SkillReward,
		Key,
		Weapon,
		Shield,
		Armor,
		Hat,
		Boots,
		Gloves,
		Accessory,
		Belt,
		Necklace,
		Ring,
		Armlet,
		Bracer,
		Beer,
		EffectPotion,
		HairDye,
		CureCurse
	}
	public enum ItemSubType : byte
	{
		None,
		Ranged,
		Arrows,
		Wings,
		//The following 2 values require modded pubs in order to work properly.
		FaceMask, //ADDED: *this* client will interpret this value as a hat/mask, so all hair should be shown (ie frog head/dragon mask)
		HideHair //ADDED: *this* client will interpret this value as a hat/helmet, so no hair should be shown (ie helmy, H.O.D., etc)
	}
	public enum ItemSpecial : byte
	{
		Normal,
		Rare, // ?
		UnknownSpecial2,
		Unique, // ?
		Lore,
		Cursed
	}
	public enum ItemSize : byte
	{
		Size1x1,
		Size1x2,
		Size1x3,
		Size1x4,
		Size2x1,
		Size2x2,
		Size2x3,
		Size2x4,
	}

	public enum NPCType
	{
		NPC,
		Passive,
		Aggressive,
		Unknown1,
		Unknown2,
		Unknown3,
		Shop,
		Inn,
		Unknown4,
		Bank,
		Barber,
		Guild,
		Priest,
		Law,
		Skills,
		Quest,
		NONE,
	}

	public enum SpellType
	{
		Heal,
		Damage,
		Bard
	}
	public enum SpellTargetRestrict
	{
		NPCOnly,
		Friendly,
		Opponent
	}
	public enum SpellTarget
	{
		Normal,
		Self,
		Unknown1,
		Group
	}

	public abstract class EODataFile
	{
		public List<IDataRecord> Data { get; protected set; }
		public string FilePath { get; protected set; }
		public int Version { get; protected set; }
		public int Rid { get; protected set; }
		public short Len { get; protected set; }

		public abstract void Load(string fName);

		/// <summary>
		/// Uses polymorphic features of the IDataType interface to save the different data types differently
		/// Headers for all types of files match, save for the first 3 bytes, which use the file extension to
		///		save the proper string.
		/// </summary>
		/// <param name="pubVersion">Version of the pub file to save. For Ethan's client, items should be 1, otherwise, this should be 0 (for now)</param>
		/// <param name="error">ref parameter that provides the Exception.Message string on an error condition</param>
		/// <returns>True if successful, false on failure. Use the 'error' parameter to check error message</returns>
		public bool Save(int pubVersion, ref string error)
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

	public interface IDataRecord
	{
		byte[] SerializeToByteArray();
		string ToString();
	}

	public class ItemRecord : IDataRecord
	{
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

		public override string ToString()
		{
			return ID + " : " + Name;
		}

		public byte[] SerializeToByteArray()
		{
			byte[] ret = new byte[ItemFile.DATA_SIZE + 1 + Name.Length];
			for (int i = 0; i < ret.Length; ++i)
				ret[i] = 254;

			using (MemoryStream mem = new MemoryStream(ret))
			{
				mem.WriteByte(Packet.EncodeNumber(Name.Length, 1)[0]);
				byte[] name = Encoding.ASCII.GetBytes(Name);
				mem.Write(name, 0, name.Length);

				mem.Write(Packet.EncodeNumber(Graphic, 2), 0, 2);
				mem.WriteByte(Packet.EncodeNumber((byte)Type, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber((byte)SubType, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber((byte)Special, 1)[0]);

				mem.Write(Packet.EncodeNumber(HP, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(TP, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(MinDam, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(MaxDam, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Accuracy, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Evade, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Armor, 2), 0, 2);

				mem.Seek(21 + Name.Length, SeekOrigin.Begin);
				mem.WriteByte(Packet.EncodeNumber(Str, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber(Int, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber(Wis, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber(Agi, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber(Con, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber(Cha, 1)[0]);

				mem.WriteByte(Packet.EncodeNumber(Light, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber(Dark, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber(Earth, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber(Air, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber(Water, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber(Fire, 1)[0]);

				mem.Write(Packet.EncodeNumber(ScrollMap, 3), 0, 3);
				mem.WriteByte(Packet.EncodeNumber(ScrollX, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber(ScrollY, 1)[0]);

				mem.Write(Packet.EncodeNumber(LevelReq, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(ClassReq, 2), 0, 2);

				mem.Write(Packet.EncodeNumber(StrReq, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(IntReq, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(WisReq, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(AgiReq, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(ConReq, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(ChaReq, 2), 0, 2);

				mem.Seek(56 + Name.Length, SeekOrigin.Begin);
				mem.WriteByte(Packet.EncodeNumber(Weight, 1)[0]);
				mem.Seek(58 + Name.Length, SeekOrigin.Begin);
				mem.WriteByte(Packet.EncodeNumber((byte)Size, 1)[0]);
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

	public class NPCRecord : IDataRecord
	{
		public int ID { get; set; }
		public string Name { get; set; }

		public int Graphic { get; set; }

		public short Boss { get; set; }
		public short Child { get; set; }
		public NPCType Type { get; set; }

		public short VendorID { get; set; }

		public int HP { get; set; }
		public ushort Exp { get; set; }
		public short MinDam { get; set; }
		public short MaxDam { get; set; }

		public short Accuracy { get; set; }
		public short Evade { get; set; }
		public short Armor { get; set; }

		public override string ToString()
		{
			return ID + ": " + Name;
		}

		public byte[] SerializeToByteArray()
		{

			byte[] ret = new byte[ClassFile.DATA_SIZE + 1 + Name.Length];
			for (int i = 0; i < ret.Length; ++i)
				ret[i] = 254;

			using (MemoryStream mem = new MemoryStream(ret))
			{
				mem.WriteByte(Packet.EncodeNumber(Name.Length, 1)[0]);
				byte[] name = Encoding.ASCII.GetBytes(Name);
				mem.Write(name, 0, name.Length);
				
				mem.Write(Packet.EncodeNumber(Graphic, 2), 0, 2);

				mem.Seek(3, SeekOrigin.Begin);
				mem.Write(Packet.EncodeNumber(Boss, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Child, 2), 0, 2);
				mem.Write(Packet.EncodeNumber((short)Type, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(VendorID, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(HP, 3), 0, 3);

				mem.Seek(16, SeekOrigin.Begin);
				mem.Write(Packet.EncodeNumber(MinDam, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(MaxDam, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Accuracy, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Evade, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Armor, 2), 0, 2);

				mem.Seek(36, SeekOrigin.Begin);
				mem.Write(Packet.EncodeNumber(Exp, 2), 0, 2);
			}

			return ret;
		}
	}

	public class SpellRecord : IDataRecord
	{
		public int ID { get; set; }
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

		public override string ToString()
		{
			return ID + ": " + Name;
		}

		public byte[] SerializeToByteArray()
		{

			byte[] ret = new byte[ClassFile.DATA_SIZE + 1 + Name.Length];
			for (int i = 0; i < ret.Length; ++i)
				ret[i] = 254;

			using (MemoryStream mem = new MemoryStream(ret))
			{
				mem.WriteByte(Packet.EncodeNumber(Name.Length, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber(Shout.Length, 1)[0]);
				byte[] name = Encoding.ASCII.GetBytes(Name);
				byte[] shout = Encoding.ASCII.GetBytes(Shout); //shout, shout, let it all out!
				mem.Write(name, 0, name.Length);
				mem.Write(shout, 0, shout.Length);
				
				mem.Write(Packet.EncodeNumber(Icon, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Graphic, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(TP, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(SP, 2), 0, 2);
				mem.WriteByte(Packet.EncodeNumber(CastTime, 1)[0]);

				mem.Seek(11, SeekOrigin.Begin);
				mem.WriteByte(Packet.EncodeNumber((byte)Type, 1)[0]);

				mem.Seek(17, SeekOrigin.Begin);
				mem.WriteByte(Packet.EncodeNumber((byte)TargetRestrict, 1)[0]);
				mem.WriteByte(Packet.EncodeNumber((byte)Target, 1)[0]);

				mem.Seek(23, SeekOrigin.Begin);
				mem.Write(Packet.EncodeNumber(MinDam, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(MaxDam, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Accuracy, 2), 0, 2);
				
				mem.Seek(34, SeekOrigin.Begin);
				mem.Write(Packet.EncodeNumber(HP, 2), 0, 2);
			}

			return ret;
		}
	}

	public class ClassRecord : IDataRecord
	{
		public int ID { get; set; }
		public string Name { get; set; }

		public byte Base { get; set; }
		public byte Type { get; set; }

		public short Str { get; set; }
		public short Int { get; set; }
		public short Wis { get; set; }
		public short Agi { get; set; }
		public short Con { get; set; }
		public short Cha { get; set; }

		public override string ToString()
		{
			return ID + ": " + Name;
		}

		public byte[] SerializeToByteArray()
		{
			
			byte[] ret = new byte[ClassFile.DATA_SIZE + 1 + Name.Length];
			for (int i = 0; i < ret.Length; ++i)
				ret[i] = 254;

			using (MemoryStream mem = new MemoryStream(ret))
			{
				mem.WriteByte(Packet.EncodeNumber(Name.Length, 1)[0]);
				byte[] name = Encoding.ASCII.GetBytes(Name);
				mem.Write(name, 0, name.Length);

				mem.WriteByte(Base);
				mem.WriteByte(Type);

				mem.Write(Packet.EncodeNumber(Str, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Int, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Wis, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Agi, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Con, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Cha, 2), 0, 2);
			}

			return ret;
		}
	}

	public class ItemFile : EODataFile
	{
		public const int DATA_SIZE = 58;

		public ItemFile()
		{
			Load(FilePath = Constants.ItemFilePath);
		}

		public ItemFile(string path)
		{
			Load(FilePath = path);
		}

		public ItemRecord GetItemRecordByID(int id)
		{
			return (ItemRecord)Data.Find(_rec => ((ItemRecord) _rec).ID == id);
		}
		public ItemRecord GetItemRecordByDollGraphic(ItemType type, short dollGraphic)
		{
			return (ItemRecord) Data.Find(_rec => ((ItemRecord) _rec).DollGraphic == dollGraphic && ((ItemRecord) _rec).Type == type);
		}
		
		public override void Load(string fPath)
		{
			using (FileStream sr = File.OpenRead(fPath)) //throw exceptions on error
			{
				sr.Seek(3, System.IO.SeekOrigin.Begin);
				
				byte[] rid = new byte[4];
				sr.Read(rid, 0, 4);
				Rid = Packet.DecodeNumber(rid);

				byte[] len = new byte[2];
				sr.Read(len, 0, 2);
				Len = (short)Packet.DecodeNumber(len);

				Data = new List<IDataRecord>(Len);
				Data.Add(new ItemRecord()); //indices are 1-based

				Version = Packet.DecodeNumber(new byte[] { (byte)sr.ReadByte() }); //this was originally seeked over
				byte[] rawData = new byte[DATA_SIZE];

				//version 0/1 support: 
				// 0 : Original EO spec
				// 1 : Ethan's updates with the 2 added SubTypes for FaceMask and HatNoHair
				if (Version == 0 || Version == 1)
				{
					for (int i = 1; i <= Len; ++i)
					{
						byte nameSize;
						nameSize = (byte)Packet.DecodeNumber(new byte[] { (byte)sr.ReadByte() });

						byte[] rawName = new byte[nameSize];
						sr.Read(rawName, 0, nameSize);
						string name = Encoding.ASCII.GetString(rawName);

						sr.Read(rawData, 0, DATA_SIZE);
						ItemRecord record = new ItemRecord()
						{
							ID = i,
							Name = name,
							Graphic = (short)Packet.DecodeNumber(rawData.SubArray(0, 2)),
							Type = (ItemType)Packet.DecodeNumber(rawData.SubArray(2, 1)),
							SubType = (ItemSubType)Packet.DecodeNumber(rawData.SubArray(3, 1)),

							Special = (ItemSpecial)Packet.DecodeNumber(rawData.SubArray(4, 1)),
							HP = (short)Packet.DecodeNumber(rawData.SubArray(5, 2)),
							TP = (short)Packet.DecodeNumber(rawData.SubArray(7, 2)),
							MinDam = (short)Packet.DecodeNumber(rawData.SubArray(9, 2)),
							MaxDam = (short)Packet.DecodeNumber(rawData.SubArray(11, 2)),
							Accuracy = (short)Packet.DecodeNumber(rawData.SubArray(13, 2)),
							Evade = (short)Packet.DecodeNumber(rawData.SubArray(15, 2)),
							Armor = (short)Packet.DecodeNumber(rawData.SubArray(17, 2)),

							Str = (byte)Packet.DecodeNumber(rawData.SubArray(20, 1)),
							Int = (byte)Packet.DecodeNumber(rawData.SubArray(21, 1)),
							Wis = (byte)Packet.DecodeNumber(rawData.SubArray(22, 1)),
							Agi = (byte)Packet.DecodeNumber(rawData.SubArray(23, 1)),
							Con = (byte)Packet.DecodeNumber(rawData.SubArray(24, 1)),
							Cha = (byte)Packet.DecodeNumber(rawData.SubArray(25, 1)),

							Light = (byte)Packet.DecodeNumber(rawData.SubArray(26, 1)),
							Dark = (byte)Packet.DecodeNumber(rawData.SubArray(27, 1)),
							Earth = (byte)Packet.DecodeNumber(rawData.SubArray(28, 1)),
							Air = (byte)Packet.DecodeNumber(rawData.SubArray(29, 1)),
							Water = (byte)Packet.DecodeNumber(rawData.SubArray(30, 1)),
							Fire = (byte)Packet.DecodeNumber(rawData.SubArray(31, 1)),

							ScrollMap = Packet.DecodeNumber(rawData.SubArray(32, 3)),
							ScrollX = (byte)Packet.DecodeNumber(rawData.SubArray(35, 1)),
							ScrollY = (byte)Packet.DecodeNumber(rawData.SubArray(36, 1)),

							LevelReq = (short)Packet.DecodeNumber(rawData.SubArray(37, 2)),
							ClassReq = (short)Packet.DecodeNumber(rawData.SubArray(39, 2)),

							StrReq = (short)Packet.DecodeNumber(rawData.SubArray(41, 2)),
							IntReq = (short)Packet.DecodeNumber(rawData.SubArray(43, 2)),
							WisReq = (short)Packet.DecodeNumber(rawData.SubArray(45, 2)),
							AgiReq = (short)Packet.DecodeNumber(rawData.SubArray(47, 2)),
							ConReq = (short)Packet.DecodeNumber(rawData.SubArray(49, 2)),
							ChaReq = (short)Packet.DecodeNumber(rawData.SubArray(51, 2)),

							Weight = (byte)Packet.DecodeNumber(rawData.SubArray(55, 1)),
							Size = (ItemSize)Packet.DecodeNumber(rawData.SubArray(57, 1))
						};

						if (record.ID == 365 && record.Name == "Gun")
							record.SubType = ItemSubType.Ranged;

						if (record.Name != "eof")
							Data.Add(record);

						if (sr.Read(new byte[1], 0, 1) != 1)
							break;
						sr.Seek(-1, SeekOrigin.Current);
					}
				}
				else
				{
					throw new FileLoadException("Unable to Load file with invalid version: " + Version);
				}
			}
		}
	}

	public class NPCFile : EODataFile
	{
		public const int DATA_SIZE = 39;

		public NPCFile()
		{
			Load(FilePath = Constants.NPCFilePath);
		}

		public NPCFile(string path)
		{
			Load(FilePath = path);
		}

		public override void Load(string fPath)
		{
			using (FileStream sr = File.OpenRead(fPath))
			{
				sr.Seek(3, SeekOrigin.Begin);

				byte[] rid = new byte[4];
				sr.Read(rid, 0, 4);
				Rid = Packet.DecodeNumber(rid);

				byte[] len = new byte[2];
				sr.Read(len, 0, 2);
				Len = (short)Packet.DecodeNumber(len);

				Data = new List<IDataRecord>(Len);
				Data.Add(new NPCRecord());

				Version = Packet.DecodeNumber(new byte[] { (byte)sr.ReadByte() });
				
				byte[] rawData = new byte[DATA_SIZE];

				if(Version == 0)
				{
					for (int i = 1; i <= Len; ++i)
					{
						byte nameSize;
						nameSize = (byte)Packet.DecodeNumber(new byte[] { (byte)sr.ReadByte() });

						byte[] rawName = new byte[nameSize];
						sr.Read(rawName, 0, nameSize);
						string name = Encoding.ASCII.GetString(rawName);

						sr.Read(rawData, 0, DATA_SIZE);
						NPCRecord record = new NPCRecord()
						{
							ID = i,
							Name = name,
							Graphic = Packet.DecodeNumber(rawData.SubArray(0,2)),
							Boss = (short)Packet.DecodeNumber(rawData.SubArray(3,2)),
							Child = (short)Packet.DecodeNumber(rawData.SubArray(5, 2)),
							Type = (NPCType)Packet.DecodeNumber(rawData.SubArray(7,2)),
							VendorID = (short)Packet.DecodeNumber(rawData.SubArray(9, 2)),
							HP = Packet.DecodeNumber(rawData.SubArray(11,3)),
							MinDam = (short)Packet.DecodeNumber(rawData.SubArray(16, 2)),
							MaxDam = (short)Packet.DecodeNumber(rawData.SubArray(18, 2)),
							Accuracy = (short)Packet.DecodeNumber(rawData.SubArray(20, 2)),
							Evade = (short)Packet.DecodeNumber(rawData.SubArray(22, 2)),
							Armor = (short)Packet.DecodeNumber(rawData.SubArray(24, 2)),
							Exp = (ushort)Packet.DecodeNumber(rawData.SubArray(36, 2)),
						};

						if (record.Name != "eof")
							Data.Add(record);

						if (sr.Read(new byte[1], 0, 1) != 1)
							break;
						sr.Seek(-1, SeekOrigin.Current);
					}
				}
				else
				{
					throw new FileLoadException("Unable to load file with invalid version: " + Version);
				}
			}
		}
	}

	public class SpellFile : EODataFile
	{
		public const int DATA_SIZE = 51;
		public SpellFile()
		{
			Load(FilePath = Constants.SpellFilePath);
		}
		public SpellFile(string path)
		{
			Load(FilePath = path);
		}

		public override void Load(string fPath)
		{
			using (FileStream sr = File.OpenRead(fPath))
			{
				sr.Seek(3, SeekOrigin.Begin);

				byte[] rid = new byte[4];
				sr.Read(rid, 0, 4);
				Rid = Packet.DecodeNumber(rid);

				byte[] len = new byte[2];
				sr.Read(len, 0, 2);
				Len = (short)Packet.DecodeNumber(len);

				Data = new List<IDataRecord>(Len);
				Data.Add(new SpellRecord());

				Version = Packet.DecodeNumber(new byte[] { (byte)sr.ReadByte() });

				byte[] rawData = new byte[DATA_SIZE];

				if (Version == 0)
				{
					for (int i = 1; i <= Len; ++i)
					{
						byte nameSize;
						nameSize = (byte)Packet.DecodeNumber(new byte[] { (byte)sr.ReadByte() });
						byte shoutSize;
						shoutSize = (byte)Packet.DecodeNumber(new byte[] { (byte)sr.ReadByte() });

						byte[] rawName = new byte[nameSize], rawShout = new byte[shoutSize];
						sr.Read(rawName, 0, nameSize);
						sr.Read(rawShout, 0, shoutSize);
						string name = Encoding.ASCII.GetString(rawName);
						string shout = Encoding.ASCII.GetString(rawShout);

						sr.Read(rawData, 0, DATA_SIZE);
						SpellRecord record = new SpellRecord()
						{
							ID = i,
							Name = name,
							Shout = shout,

							Icon = (short)Packet.DecodeNumber(rawData.SubArray(0, 2)),
							Graphic = (short)Packet.DecodeNumber(rawData.SubArray(2, 2)),
							TP = (short)Packet.DecodeNumber(rawData.SubArray(4, 2)),
							SP = (short)Packet.DecodeNumber(rawData.SubArray(6, 2)),
							CastTime = (byte)Packet.DecodeNumber(new byte[] { rawData[8] }),

							Type = (SpellType)Packet.DecodeNumber(new byte[] { rawData[11] }),
							TargetRestrict = (SpellTargetRestrict)Packet.DecodeNumber(new byte[] { rawData[17] }),
							Target = (SpellTarget)Packet.DecodeNumber(new byte[] { rawData[18] }),

							MinDam = (short)Packet.DecodeNumber(rawData.SubArray(23, 2)),
							MaxDam = (short)Packet.DecodeNumber(rawData.SubArray(25, 2)),
							Accuracy = (short)Packet.DecodeNumber(rawData.SubArray(27, 2)),
							HP = (short)Packet.DecodeNumber(rawData.SubArray(34, 2))
						};

						if (record.Name != "eof")
							Data.Add(record);

						if (sr.Read(new byte[1], 0, 1) != 1)
							break;
						sr.Seek(-1, SeekOrigin.Current);
					}
				}
				else
				{
					throw new FileLoadException("Unable to load file with invalid version: " + Version);
				}
			}
		}

		public SpellRecord GetSpellRecordByID(short id)
		{
			return (SpellRecord) Data.Find(x => ((SpellRecord) x).ID == id);
		}
	}

	public class ClassFile : EODataFile
	{
		public const int DATA_SIZE = 14;

		public ClassFile()
		{
			Load(FilePath = Constants.ClassFilePath);
		}
		public ClassFile(string path)
		{
			Load(FilePath = path);
		}

		public override void Load(string fPath)
		{
			using (FileStream sr = File.OpenRead(fPath))
			{
				sr.Seek(3, SeekOrigin.Begin);

				byte[] rid = new byte[4];
				sr.Read(rid, 0, 4);
				Rid = Packet.DecodeNumber(rid);

				byte[] len = new byte[2];
				sr.Read(len, 0, 2);
				Len = (short)Packet.DecodeNumber(len);

				Data = new List<IDataRecord>(Len);
				Data.Add(new ClassRecord());

				Version = Packet.DecodeNumber(new byte[] { (byte)sr.ReadByte() });

				byte[] rawData = new byte[DATA_SIZE];

				if (Version == 0)
				{
					for (int i = 1; i <= Len; ++i)
					{
						byte nameSize;
						nameSize = (byte)Packet.DecodeNumber(new byte[] { (byte)sr.ReadByte() });

						byte[] rawName = new byte[nameSize];
						sr.Read(rawName, 0, nameSize);
						string name = Encoding.ASCII.GetString(rawName);

						sr.Read(rawData, 0, DATA_SIZE);
						ClassRecord record = new ClassRecord()
						{
							ID = i,
							Name = name,
							Base = (byte)Packet.DecodeNumber(new byte[] {rawData[0]}),
							Type = (byte)Packet.DecodeNumber(new byte[] {rawData[1]}),
							Str = (short)Packet.DecodeNumber(rawData.SubArray(2, 2)),
							Int = (short)Packet.DecodeNumber(rawData.SubArray(4,2)),
							Wis = (short)Packet.DecodeNumber(rawData.SubArray(6,2)),
							Agi = (short)Packet.DecodeNumber(rawData.SubArray(8,2)),
							Con = (short)Packet.DecodeNumber(rawData.SubArray(10,2)),
							Cha = (short)Packet.DecodeNumber(rawData.SubArray(12,2))
						};

						if (record.Name != "eof")
							Data.Add(record);

						if (sr.Read(new byte[1], 0, 1) != 1)
							break;
						sr.Seek(-1, SeekOrigin.Current);
					}
				}
				else
				{
					throw new FileLoadException("Unable to load file with invalid version: " + Version);
				}
			}
		}
	}

	public class EDFFile
	{
		public int ID { get; private set; }

		public Dictionary<int, string> Data { get; private set; } 

		public EDFFile(string fileName, DataFiles whichFile)
		{
			if(!File.Exists(fileName))
				throw new FileNotFoundException("File does not exist!", fileName);

			int lastSlash = fileName.LastIndexOf('\\');
			if (lastSlash < 0)
				lastSlash = fileName.LastIndexOf('/');

			ID = int.Parse(fileName.Substring(lastSlash < 0 ? 0 : lastSlash + 4, 3));
			Data = new Dictionary<int, string>();

			if (whichFile == DataFiles.CurseFilter)
			{
				string[] lines = File.ReadAllLines(fileName);
				int i = 0;
				foreach (string encoded in lines)
				{
					string decoded = _decodeDatString(encoded, whichFile);
					string[] curses = decoded.Split(new[] {':'});
					foreach(string curse in curses)
						Data.Add(i++, curse);
				}
			}
			else
			{
				string[] lines = File.ReadAllLines(fileName, Encoding.Default);
				int i = 0;
				foreach (string encoded in lines)
					Data.Add(i++, _decodeDatString(encoded, whichFile));
			}
		}

		private string _decodeDatString(string input, DataFiles whichFile)
		{
			//unencrypted
			if (whichFile == DataFiles.Credits || whichFile == DataFiles.Checksum)
				return input;

			string ret = "";

			for (int i = 0; i < input.Length; i += 2)
				ret += input[i];

			//if there are an even number of characters start with the last one
			//otherwise start with the second to last one
			int startIndex = input.Length - (input.Length%2 == 0 ? 1 : 2);
			for (int i = startIndex; i >= 0; i -= 2)
				ret += input[i];

			if (whichFile == DataFiles.CurseFilter)
				return ret;

			StringBuilder sb = new StringBuilder(ret);
			//additional unscrambling (testing/WIP!)
			//adjacent ascii char values that are multiples of 7, starting at ? (63), should be flipped
			for (int i = 0; i < sb.Length; ++i)
			{
				int next = i + 1;
				if (next < sb.Length)
				{
					char c1 = sb[i], c2 = sb[next];
					int ch1 = Convert.ToInt32(c1);
					int ch2 = Convert.ToInt32(c2);

					if (ch1%7 == 0 && ch2%7 == 0)
					{
						sb[i] = c2;
						sb[next] = c1;
					}
				}
			}

			return sb.ToString();
		}
	}
}
