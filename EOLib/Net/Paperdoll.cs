// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.ObjectModel;
using System.Linq;
using EOLib.IO;

namespace EOLib.Net
{
	public struct PaperdollEquipData
	{
		private readonly bool itemRemoved;
		private readonly short itemID;
		private readonly int characterAmount;
		private readonly byte subLoc;
		private readonly short maxhp, maxtp, disp_str, disp_int, disp_wis,
			disp_agi, disp_con, disp_cha, mindam, maxdam, accuracy, evade, armor;

		/// <summary>
		/// This structure contains data about an item that was unequipped
		/// </summary>
		public bool ItemWasUnequipped { get { return itemRemoved; } }

		/// <summary>
		/// Item ID of Item that was equipped
		/// </summary>
		public short ItemID { get { return itemID; } }

		/// <summary>
		/// Amount of item remaining in inventory
		/// </summary>
		public int ItemAmount { get { return characterAmount; } }

		public byte SubLoc { get { return subLoc; } }

		public short MaxHP { get { return maxhp; } }
		public short MaxTP { get { return maxtp; } }
		public short Str { get { return disp_str; } }
		public short Int { get { return disp_int; } }
		public short Wis { get { return disp_wis; } }
		public short Agi { get { return disp_agi; } }
		public short Con { get { return disp_con; } }
		public short Cha { get { return disp_cha; } }

		public short MinDam { get { return mindam; } }
		public short MaxDam { get { return maxdam; } }
		public short Accuracy { get { return accuracy; } }
		public short Evade { get { return evade; } }
		public short Armor { get { return armor; } }

		internal PaperdollEquipData(Packet pkt, bool itemUnequipped)
		{
			itemRemoved = itemUnequipped;

			itemID = pkt.GetShort();
			characterAmount = itemUnequipped ? 1 : pkt.GetThree();
			subLoc = pkt.GetChar();

			maxhp = pkt.GetShort();
			maxtp = pkt.GetShort();
			disp_str = pkt.GetShort();
			disp_int = pkt.GetShort();
			disp_wis = pkt.GetShort();
			disp_agi = pkt.GetShort();
			disp_con = pkt.GetShort();
			disp_cha = pkt.GetShort();
			mindam = pkt.GetShort();
			maxdam = pkt.GetShort();
			accuracy = pkt.GetShort();
			evade = pkt.GetShort();
			armor = pkt.GetShort();
		}
	}

	public struct PaperdollDisplayData
	{
		private readonly string name, home, partner, title, guild, rank;
		private readonly short playerID;
		private readonly byte clas, gender;
		private readonly short[] paperdoll;
		private readonly PaperdollIconType iconType;

		public string Name { get { return name; } }
		public string Home { get { return home; } }
		public string Partner { get { return partner; } }
		public string Title { get { return title; } }
		public string Guild { get { return guild; } }
		public string Rank { get { return rank; } }

		public short PlayerID { get { return playerID; } }
		public byte Class { get { return clas; } }
		public byte Gender { get { return gender; } }
		public ReadOnlyCollection<short> Paperdoll { get { return paperdoll.AsEnumerable().ToList().AsReadOnly(); } }
		public PaperdollIconType Icon { get { return iconType; } }

		internal PaperdollDisplayData(Packet pkt)
		{
			//need to be applied to the character that is passed to the dialog
			name = pkt.GetBreakString();
			home = pkt.GetBreakString();
			partner = pkt.GetBreakString();
			title = pkt.GetBreakString();
			guild = pkt.GetBreakString();
			rank = pkt.GetBreakString();

			playerID = pkt.GetShort();
			clas = pkt.GetChar();
			gender = pkt.GetChar();

			if (pkt.GetChar() != 0)
				throw new ArgumentException("Invalid/malformed packet", "pkt");

			paperdoll = new short[(int)EquipLocation.PAPERDOLL_MAX];
			for (int i = 0; i < (int)EquipLocation.PAPERDOLL_MAX; ++i)
				paperdoll[i] = pkt.GetShort();

			iconType = (PaperdollIconType)pkt.GetChar();
		}
	}

	partial class PacketAPI
	{
		public delegate void PaperdollChangeEvent(PaperdollEquipData data);
		public delegate void PaperdollShowEvent(PaperdollDisplayData data);
		public event PaperdollChangeEvent OnPlayerPaperdollChange;
		public event PaperdollShowEvent OnViewPaperdoll;

		private void _createPaperdollMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.PaperDoll, PacketAction.Agree), _handlePaperdollAgree, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.PaperDoll, PacketAction.Remove), _handlePaperdollRemove, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.PaperDoll, PacketAction.Reply), _handlePaperdollReply, true);
		}

		public bool EquipItem(short id, byte subLoc = 0)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized) return false;

			Packet pkt = new Packet(PacketFamily.PaperDoll, PacketAction.Add);
			pkt.AddShort(id);
			pkt.AddChar(subLoc);

			return m_client.SendPacket(pkt);
		}

		public bool UnequipItem(short id, byte subLoc = 0)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized) return false;

			Packet pkt = new Packet(PacketFamily.PaperDoll, PacketAction.Remove);
			pkt.AddShort(id);
			pkt.AddChar(subLoc);

			return m_client.SendPacket(pkt);
		}

		public bool RequestPaperdoll(short charId)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized) return false;

			Packet pkt = new Packet(PacketFamily.PaperDoll, PacketAction.Request);
			pkt.AddShort(charId);

			return m_client.SendPacket(pkt);
		}
		
		//this is only ever sent to MainPlayer (avatar handles other players)
		private void _handlePaperdollAgree(Packet pkt)
		{
			if (OnPlayerPaperdollChange == null) return;

			_handleAvatarAgree(pkt); //same logic in the beginning of the packet

			PaperdollEquipData data = new PaperdollEquipData(pkt, false);
			OnPlayerPaperdollChange(data);
		}

		//this is only ever sent to MainPlayer (avatar handles other players)
		private void _handlePaperdollRemove(Packet pkt)
		{
			if (OnPlayerPaperdollChange == null) return;

			//the $strip command does this wrong (adding 0's in), somehow the original client is smart enough to figure it out
			//normally would put this block in the _handleAvatarAgree
			short playerID = pkt.GetShort();
			AvatarSlot slot = (AvatarSlot) pkt.GetChar();
			bool sound = pkt.GetChar() == 0; //sound : 0
			
			short boots = pkt.GetShort();
			if (pkt.Length != 45) pkt.Skip(sizeof(short) * 3); //three 0s
			short armor = pkt.GetShort();
			if (pkt.Length != 45) pkt.Skip(sizeof(short)); // one 0
			short hat = pkt.GetShort();
			short shield, weapon;
			if (pkt.Length != 45)
			{
				shield = pkt.GetShort();
				weapon = pkt.GetShort();
			}
			else
			{
				weapon = pkt.GetShort();
				shield = pkt.GetShort();
			}

			AvatarData renderData = new AvatarData(playerID, slot, sound, boots, armor, hat, weapon, shield);
			if (OnPlayerAvatarChange != null)
				OnPlayerAvatarChange(renderData);

			PaperdollEquipData data = new PaperdollEquipData(pkt, true);
			OnPlayerPaperdollChange(data);
		}

		private void _handlePaperdollReply(Packet pkt) //sent when showing a paperdoll for a character
		{
			if (OnViewPaperdoll == null) return;

			PaperdollDisplayData data;
			try
			{
				data = new PaperdollDisplayData(pkt);
			}
			catch (ArgumentException)
			{
				return;
			}
			OnViewPaperdoll(data);
		}
	}
}
