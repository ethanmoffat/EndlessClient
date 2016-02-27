// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Threading;
using EOLib.IO;

namespace EOLib.Net.API
{
	internal enum WelcomeReply : short
	{
		RequestGranted = 1, //response from welcome_request
		WelcomeMessage = 2, //response from welcome_message
	}

	public struct InventoryItem
	{
		public short id;
		public int amount;
	}
	public struct CharacterSpell
	{
		public short id;
		public short level;
	}

	public class WelcomeRequestData
	{
		public short PlayerID { get; private set; }
		public int ActiveCharacterID { get; private set; }
		
		public short MapID { get; private set; }
		public byte[] MapRID { get; private set; }
		public int MapLen { get; private set; }
		public bool MapIsPK { get; private set; }

		public int EifRid { get; private set; }
		public short EifLen { get; private set; }
		public int EnfRid { get; private set; }
		public short EnfLen { get; private set; }
		public int EsfRid { get; private set; }
		public short EsfLen { get; private set; }
		public int EcfRid { get; private set; }
		public short EcfLen { get; private set; }

		public string Name { get; private set; }
		public string Title { get; private set; }
		public string GuildName { get; private set; }
		public string GuildRankStr { get; private set; }
		public byte ClassID { get; private set; }
		public string PaddedGuildTag { get; private set; }
		public AdminLevel AdminLevel { get; private set; }

		public byte Level { get; private set; }
		public int Exp { get; private set; }
		public int Usage { get; private set; }

		public short HP { get; private set; }
		public short MaxHP { get; private set; }
		public short TP { get; private set; }
		public short MaxTP { get; private set; }
		public short MaxSP { get; private set; }

		public short StatPoints { get; private set; }
		public short SkillPoints { get; private set; }
		public short Karma { get; private set; }
		public short MinDam { get; private set; }
		public short MaxDam { get; private set; }
		public short Accuracy { get; private set; }
		public short Evade { get; private set; }
		public short Armor { get; private set; }
		public short DispStr { get; private set; }
		public short DispInt { get; private set; }
		public short DispWis { get; private set; }
		public short DispAgi { get; private set; }
		public short DispCon { get; private set; }
		public short DispCha { get; private set; }

		public short[] PaperDoll { get; private set; }

		public byte GuildRankNum { get; private set; }
		public short JailMap { get; private set; }

		public bool FirstTimePlayer { get; private set; }

		internal WelcomeRequestData(OldPacket pkt)
		{
			PlayerID = pkt.GetShort();
			ActiveCharacterID = pkt.GetInt();

			MapID = pkt.GetShort();
			MapRID = pkt.GetBytes(4);
			MapLen = pkt.GetThree();
			MapIsPK = MapRID[0] == 0xFF && MapRID[1] == 0x01;

			EifRid = OldPacket.DecodeNumber(pkt.GetBytes(4));
			EifLen = (short)OldPacket.DecodeNumber(pkt.GetBytes(2));

			EnfRid = OldPacket.DecodeNumber(pkt.GetBytes(4));
			EnfLen = (short)OldPacket.DecodeNumber(pkt.GetBytes(2));

			EsfRid = OldPacket.DecodeNumber(pkt.GetBytes(4));
			EsfLen = (short)OldPacket.DecodeNumber(pkt.GetBytes(2));

			EcfRid = OldPacket.DecodeNumber(pkt.GetBytes(4));
			EcfLen = (short)OldPacket.DecodeNumber(pkt.GetBytes(2));

			Name = pkt.GetBreakString();
			Title = pkt.GetBreakString();
			GuildName = pkt.GetBreakString();
			GuildRankStr = pkt.GetBreakString();
			ClassID = pkt.GetChar();
			PaddedGuildTag = pkt.GetFixedString(3); //padded guild tag is 3 characters
			AdminLevel = (AdminLevel)pkt.GetChar();

			Level = pkt.GetChar();
			Exp = pkt.GetInt();
			Usage = pkt.GetInt();

			HP = pkt.GetShort();
			MaxHP = pkt.GetShort();
			TP = pkt.GetShort();
			MaxTP = pkt.GetShort();
			MaxSP = pkt.GetShort();

			StatPoints = pkt.GetShort();
			SkillPoints = pkt.GetShort();
			Karma = pkt.GetShort();
			MinDam = pkt.GetShort();
			MaxDam = pkt.GetShort();
			Accuracy = pkt.GetShort();
			Evade = pkt.GetShort();
			Armor = pkt.GetShort();
			DispStr = pkt.GetShort();
			DispInt = pkt.GetShort();
			DispWis = pkt.GetShort();
			DispAgi = pkt.GetShort();
			DispCon = pkt.GetShort();
			DispCha = pkt.GetShort();

			PaperDoll = new short[(int)EquipLocation.PAPERDOLL_MAX];
			for (int i = 0; i < (int)EquipLocation.PAPERDOLL_MAX; ++i)
			{
				PaperDoll[i] = pkt.GetShort();
			}

			GuildRankNum = pkt.GetChar();
			JailMap = pkt.GetShort();
			pkt.Skip(12); //i think these can safely be skipped for the moment
			FirstTimePlayer = pkt.GetChar() == 2; //signal that the player should see the "first timer" message
		}
	}

	public class WelcomeMessageData
	{
		private readonly List<string> m_news; 
		public IList<string> News { get { return m_news.AsReadOnly(); } }

		public byte Weight { get; private set; }
		public byte MaxWeight { get; private set; }

		private readonly List<InventoryItem> m_inventory;
		public IEnumerable<InventoryItem> Inventory { get { return m_inventory.AsReadOnly(); } }

		private readonly List<CharacterSpell> m_spells;
		public IEnumerable<CharacterSpell> Spells { get { return m_spells.AsReadOnly(); } }

		private readonly List<CharacterData> m_characters;
		private readonly List<NPCData> m_npcs;
		private readonly List<MapItem> m_items;

		public IEnumerable<CharacterData> CharacterData { get { return m_characters.AsReadOnly(); } }
		public IEnumerable<NPCData> NPCData { get { return m_npcs.AsReadOnly(); } }
		public IEnumerable<MapItem> MapItemData { get { return m_items.AsReadOnly(); } }

		internal WelcomeMessageData(OldPacket pkt)
		{
			m_news = new List<string>();
			for (int i = 0; i < 9; ++i)
			{
				m_news.Add(pkt.GetBreakString());
			}

			Weight = pkt.GetChar();
			MaxWeight = pkt.GetChar();

			m_inventory = new List<InventoryItem>();
			while (pkt.PeekByte() != 255)
				m_inventory.Add(new InventoryItem { id = pkt.GetShort(), amount = pkt.GetInt() });
			pkt.GetByte();

			m_spells = new List<CharacterSpell>();
			while (pkt.PeekByte() != 255)
				m_spells.Add(new CharacterSpell { id = pkt.GetShort(), level = pkt.GetShort() });
			pkt.GetByte();

			//Get data for other characters
			int numOtherCharacters = pkt.GetChar();
			m_characters = new List<CharacterData>(numOtherCharacters);
			if (pkt.GetByte() != 255) throw new Exception();
			for (int i = 0; i < numOtherCharacters; ++i)
			{
				CharacterData newGuy = new CharacterData(pkt);
				if (pkt.GetByte() != 255)
					throw new Exception();

				m_characters.Add(newGuy);
			}

			//get data for any npcs
			m_npcs = new List<NPCData>();
			while (pkt.PeekByte() != 255)
			{
				NPCData newGuy = new NPCData(pkt);
				m_npcs.Add(newGuy);
			}
			pkt.GetByte();

			//get data for items on map
			m_items = new List<MapItem>();
			while (pkt.ReadPos < pkt.Length)
			{
				m_items.Add(new MapItem
				{
					uid = pkt.GetShort(),
					id = pkt.GetShort(),
					x = pkt.GetChar(),
					y = pkt.GetChar(),
					amount = pkt.GetThree(),
					//turn off drop protection for items coming into view - server will validate
					time = DateTime.Now.AddSeconds(-5),
					npcDrop = false,
					playerID = -1
				});
			}
		}
	}

	partial class PacketAPI
	{
		private AutoResetEvent m_welcome_responseEvent;

		private WelcomeRequestData m_welcome_requestData;
		private WelcomeMessageData m_welcome_messageData;

		private void _createWelcomeMembers()
		{
			m_welcome_responseEvent = new AutoResetEvent(false);
			m_welcome_requestData = null;
			m_welcome_messageData = null;

			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Welcome, PacketAction.Reply), _handleWelcomeReply, false);
		}

		private void _disposeWelcomeMembers()
		{
			if (m_welcome_responseEvent != null)
			{
				m_welcome_responseEvent.Dispose();
				m_welcome_responseEvent = null;
			}
		}

		/// <summary>
		/// Select one of the 3 characters. Sends a WELCOME_REQUEST packet.
		/// </summary>
		/// <param name="id">id of the selected character</param>
		/// <param name="data">returned data from the server</param>
		/// <returns>true on successful transfer, false otherwise</returns>
		public bool SelectCharacter(int id, out WelcomeRequestData data)
		{
			data = null;
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			OldPacket builder = new OldPacket(PacketFamily.Welcome, PacketAction.Request);
			builder.AddInt(id);

			if (!m_client.SendPacket(builder))
				return false;

			if (!m_welcome_responseEvent.WaitOne(Constants.ResponseTimeout))
				return false;

			data = m_welcome_requestData;

			return true;
		}

		public bool WelcomeMessage(int id, out WelcomeMessageData data)
		{
			data = null;
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			OldPacket builder = new OldPacket(PacketFamily.Welcome, PacketAction.Message);
			builder.AddThree(0x00123456); //?
			builder.AddInt(id);

			if (!m_client.SendPacket(builder))
				return false;

			if (!m_welcome_responseEvent.WaitOne(Constants.ResponseTimeout))
				return false;

			data = m_welcome_messageData;
			m_client.IsInGame = true;

			return true;
		}

		private void _handleWelcomeReply(OldPacket pkt)
		{
			WelcomeReply reply = (WelcomeReply) pkt.GetShort();
			
			bool success;
			switch (reply)
			{
				case WelcomeReply.RequestGranted:
					m_welcome_requestData = new WelcomeRequestData(pkt);
					success = pkt.GetByte() == 255;
					break;
				case WelcomeReply.WelcomeMessage:
					if (pkt.GetByte() != 255) return; //error, something is off.
					try
					{
						m_welcome_messageData = new WelcomeMessageData(pkt);
						success = true;
					}
					catch { success = false; }
					break;
				default:
					success = false; //malformed packet
					break;
			}

			if(success)
				m_welcome_responseEvent.Set();
		}
	}
}
