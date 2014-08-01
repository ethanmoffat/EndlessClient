using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace EOLib.Data
{
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
	
	enum NPCType
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

	enum SpellType
	{
		Damage,
		Heal,
		Bard
	}
	enum SpellTargetRestrict
	{
		Any,
		Friendly,
		Opponent
	}
	enum SpellTarget
	{
		Normal,
		Self,
		Unknown1,
		Group
	}

	//biggest pain in the ass ever. And, I thought of a better way to do it as soon as I was done.
	//I'm not going to just throw all that work away though ;P 
	[StructLayout(LayoutKind.Explicit)] //porting the unions from C++ code in eoserv
	public class ItemRecord
	{
		//FieldOffset attributes can't be used on C# Properties...
		//So private members were added for each Property in the struct
		[FieldOffset(0)]
		private int _id;
		public int ID { get { return _id; } set { _id = value; } }
		[FieldOffset(4)]
		private short _graphic;
		public short Graphic { get { return _graphic; } set { _graphic = value; } }
		[FieldOffset(6)]
		private ItemType _type;
		public ItemType Type { get { return _type; } set { _type = value; } }
		[FieldOffset(7)]
		private ItemSubType _subtype;
		public ItemSubType SubType { get { return _subtype; } set { _subtype = value; } }

		[FieldOffset(8)]
		private ItemSpecial _special;
		public ItemSpecial Special { get { return _special; } set { _special = value; } }
		[FieldOffset(10)]
		private short _hp;
		public short HP { get { return _hp; } set { _hp = value; } }
		[FieldOffset(12)]
		private short _tp;
		public short TP { get { return _tp; } set { _tp = value; } }
		[FieldOffset(14)]
		private short _mindam;
		public short MinDam { get { return _mindam; } set { _mindam = value; } }
		[FieldOffset(16)]
		private short _maxdam;
		public short MaxDam { get { return _maxdam; } set { _maxdam = value; } }
		[FieldOffset(18)]
		private short _accuracy;
		public short Accuracy { get { return _accuracy; } set { _accuracy = value; } }
		[FieldOffset(20)]
		private short _evade;
		public short Evade { get { return _evade; } set { _evade = value; } }
		[FieldOffset(22)]
		private short _armor;
		public short Armor { get { return _armor; } set { _armor = value; } }

		[FieldOffset(24)]
		private byte _str;
		public byte Str { get { return _str; } set { _str = value; } }
		[FieldOffset(25)]
		private byte _int;
		public byte Int { get { return _int; } set { _int = value; } }
		[FieldOffset(26)]
		private byte _wis;
		public byte Wis { get { return _wis; } set { _wis = value; } }
		[FieldOffset(27)]
		private byte _agi;
		public byte Agi { get { return _agi; } set { _agi = value; } }
		[FieldOffset(28)]
		private byte _con;
		public byte Con { get { return _con; } set { _con = value; } }
		[FieldOffset(29)]
		private byte _cha;
		public byte Cha { get { return _cha; } set { _cha = value; } }

		[FieldOffset(30)]
		private byte _light;
		public byte Light { get { return _light; } set { _light = value; } }
		[FieldOffset(31)]
		private byte _dark;
		public byte Dark { get { return _dark; } set { _dark = value; } }
		[FieldOffset(32)]
		private byte _earth;
		public byte Earth { get { return _earth; } set { _earth = value; } }
		[FieldOffset(33)]
		private byte _air;
		public byte Air { get { return _air; } set { _air = value; } }
		[FieldOffset(34)]
		private byte _water;
		public byte Water { get { return _water; } set { _water= value; } }
		[FieldOffset(35)]
		private byte _fire;
		public byte Fire { get { return _fire; } set { _fire = value; } }

		//THESE ALL POINT TO THE SAME MEMORY
		//THIS IS BY DESIGN BECAUSE APPARENTLY EO IS LITERALLY RETARDED
		[FieldOffset(36)]
		private int _scrollmap;
		public int ScrollMap { get { return _scrollmap; } set { _scrollmap = value; } }
		[FieldOffset(36)]
		private int _dollgraphic;
		public int DollGraphic { get { return _dollgraphic; } set { _dollgraphic = value; } }
		[FieldOffset(36)]
		private int _expreward;
		public int ExpReward { get { return _expreward; } set { _expreward = value; } }
		[FieldOffset(36)]
		private int _haircolor;
		public int HairColor { get { return _haircolor; } set { _haircolor = value; } }
		[FieldOffset(36)]
		private int _effect;
		public int Effect { get { return _effect; } set { _effect = value; } }
		[FieldOffset(36)]
		private int _key;
		public int Key { get { return _key; } set { _key = value; } }

		[FieldOffset(40)]
		private byte _gender;
		public byte Gender { get { return _gender; } set { _gender = value; } }
		[FieldOffset(40)]
		private byte _scrollx;
		public byte ScrollX { get { return _scrollx; } set { _scrollx = value; } }

		[FieldOffset(41)]
		private byte _scrolly;
		public byte ScrollY { get { return _scrolly; } set { _scrolly = value; } }
		[FieldOffset(41)]
		private byte _dualwielddollgraphic;
		public byte DualWieldDollGraphic { get { return _dualwielddollgraphic; } set { _dualwielddollgraphic = value; } }

		[FieldOffset(42)]
		private short _levelreq;
		public short LevelReq { get { return _levelreq; } set { _levelreq = value; } }
		[FieldOffset(44)]
		private short _classreq;
		public short ClassReq { get { return _classreq; } set { _classreq = value; } }
		[FieldOffset(46)]
		private short _strreq;
		public short StrReq { get { return _strreq; } set { _strreq = value; } }
		[FieldOffset(48)]
		private short _intreq;
		public short IntReq { get { return _intreq; } set { _intreq = value; } }
		[FieldOffset(50)]
		private short _wisreq;
		public short WisReq { get { return _wisreq; } set { _wisreq = value; } }
		[FieldOffset(52)]
		private short _agireq;
		public short AgiReq { get { return _agireq; } set { _agireq = value; } }
		[FieldOffset(54)]
		private short _conreq;
		public short ConReq { get { return _conreq; } set { _conreq = value; } }
		[FieldOffset(56)]
		private short _chareq;
		public short ChaReq { get { return _chareq; } set { _chareq = value; } }

		[FieldOffset(58)]
		private byte _weight;
		public byte Weight { get { return _weight; } set { _weight = value; } }

		[FieldOffset(59)]
		private ItemSize _size;
		public ItemSize Size { get { return _size; } set { _size = value; } }

		[FieldOffset(60)]
		private string _name;
		public string Name { get { return _name; } set { _name = value; } } //name has to go last for field offsets to work

		public override string ToString()
		{
			return ID + " : " + Name;
		}

		public byte[] SerializeToByteArray()
		{
			byte[] ret = new byte[59 + Name.Length];
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
	}

	public class ItemFile
	{
		public List<ItemRecord> Data { get; private set; }

		public string FilePath { get; private set; }

		public int Version { get; private set; }

		public ItemFile()
		{
			load(FilePath = Constants.ItemFilePath);
		}

		public ItemFile(string path)
		{
			load(FilePath = path);
		}

		private void load(string fPath)
		{
			using (FileStream sr = File.OpenRead(fPath)) //throw exceptions on error
			{
				sr.Seek(3, System.IO.SeekOrigin.Begin);
				sr.Seek(4, SeekOrigin.Current); //skip over rid, it isn't used

				byte[] len = new byte[2];
				sr.Read(len, 0, 2);

				int max = Packet.DecodeNumber(len);
				Data = new List<ItemRecord>(max);
				Data.Add(new ItemRecord()); //indices are 1-based

				Version = Packet.DecodeNumber(new byte[] { (byte)sr.ReadByte() }); //this was originally seeked over
				byte[] rawData = new byte[58];

				//version 0/1 support: 
				// 0 : Original EO spec
				// 1 : Ethan's updates with the 2 added SubTypes for FaceMask and HatNoHair
				if (Version == 0 || Version == 1)
				{
					for (int i = 1; i <= max; ++i)
					{
						byte nameSize;
						nameSize = (byte)Packet.DecodeNumber(new byte[] { (byte)sr.ReadByte() });

						byte[] rawName = new byte[nameSize];
						sr.Read(rawName, 0, nameSize);
						string name = Encoding.ASCII.GetString(rawName);

						sr.Read(rawData, 0, 58);
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
					throw new FileLoadException("Unable to load file with invalid version: " + Version);
				}
			}
		}

		public bool Save(int pubVersion, ref string error)
		{
			try
			{
				using (FileStream sw = File.Create(FilePath)) //throw exceptions on error
				{
					sw.Write(new byte[] { (byte)'E', (byte)'I', (byte)'F' }, 0, 3); //EIF at beginning
					sw.Write(new byte[] { (byte)0x1f, (byte)0xfe, (byte)0xfe, (byte)0xfe }, 0, 4); //rid
					sw.Write(Packet.EncodeNumber(Data.Count, 2), 0, 2); //len

					Version = pubVersion;
					sw.WriteByte(Packet.EncodeNumber(Version, 1)[0]); //new version check courtesy of ME

					for (int i = 1; i < Data.Count; ++i)
					{
						if (Data[i].Name == "eof")
							break;

						byte[] toWrite = Data[i].SerializeToByteArray(); 
						sw.Write(toWrite, 0, toWrite.Length);
					}
				}
			}
			catch(Exception ex)
			{
				error = ex.Message;
				return false;
			}

			error = "none";
			return true;
		}
	}
}
