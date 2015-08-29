using System;
using EOLib.Data;

namespace EOLib.Net
{
	public struct LevelUpStats
	{
		private readonly int exp;
		private readonly byte level;
		private readonly short stat, skill, maxhp, maxtp, maxsp;

		public int Exp { get { return exp; } }
		public byte Level { get { return level; } }
		public short StatPoints { get { return stat; } }
		public short SkillPoints { get { return skill; } }
		public short MaxHP { get { return maxhp; } }
		public short MaxTP { get { return maxtp; } }
		public short MaxSP { get { return maxsp; } }

		internal LevelUpStats(Packet pkt, bool includeExp)
		{
			//includeExp will be false when leveling up from NPC, true from EXPReward
			//NPC handler happens slightly differently
			exp = includeExp ? pkt.GetInt() : 0;
			level = pkt.GetChar();
			stat = pkt.GetShort();
			skill = pkt.GetShort();
			maxhp = pkt.GetShort();
			maxtp = pkt.GetShort();
			maxsp = pkt.GetShort();
		}
	}

	public struct ItemUseData
	{
		public struct CureCurseStats
		{
			private readonly short m_maxhp, m_maxtp;
			private readonly short m_str, m_int, m_wis, m_agi, m_con, m_cha;
			private readonly short m_mindam, m_maxdam, m_acc, m_evade, m_armor;

			public short MaxHP { get { return m_maxhp; } }
			public short MaxTP { get { return m_maxtp; } }
			public short Str { get { return m_str; } }
			public short Int { get { return m_int; } }
			public short Wis { get { return m_wis; } }
			public short Agi { get { return m_agi; } }
			public short Con { get { return m_con; } }
			public short Cha { get { return m_cha; } }
			public short MinDam { get { return m_mindam; } }
			public short MaxDam { get { return m_maxdam; } }
			public short Accuracy { get { return m_acc; } }
			public short Evade { get { return m_evade; } }
			public short Armor { get { return m_armor; } }

			internal CureCurseStats(Packet pkt)
			{
				m_maxhp = pkt.GetShort();
				m_maxtp = pkt.GetShort();
				m_str = pkt.GetShort();
				m_int = pkt.GetShort();
				m_wis = pkt.GetShort();
				m_agi = pkt.GetShort();
				m_con = pkt.GetShort();
				m_cha = pkt.GetShort();
				m_mindam = pkt.GetShort();
				m_maxdam = pkt.GetShort();
				m_acc = pkt.GetShort();
				m_evade = pkt.GetShort();
				m_armor = pkt.GetShort();
			}
		}

		//in every packet
		private readonly ItemType type;
		private readonly short itemID;
		private readonly int characterAmount;
		private readonly byte weight, maxWeight;
		public ItemType Type { get { return type; } }
		public short ItemID { get { return itemID; } }
		public int CharacterAmount { get { return characterAmount; } }
		public byte Weight { get { return weight; } }
		public byte MaxWeight { get { return maxWeight; } }

		//heal type
		private readonly int hpGain;
		private readonly short hp, tp;
		public int HPGain { get { return hpGain; } }
		public short HP { get { return hp; } }
		public short TP { get { return tp; } }

		//hairdye type
		private readonly byte hairColor;
		public byte HairColor { get { return hairColor; } }

		//effect potion type
		private readonly short effect;
		public short EffectID { get { return effect; } }

		//curecurse type
		private readonly CureCurseStats? curecurse_stats;
		public CureCurseStats CureStats { get { return curecurse_stats ?? new CureCurseStats(); } }

		//expreward type
		private readonly LevelUpStats? expreward_stats;
		public LevelUpStats RewardStats { get { return expreward_stats ?? new LevelUpStats(); } }

		internal ItemUseData(Packet pkt)
		{
			type = (ItemType)pkt.GetChar();
			itemID = pkt.GetShort();
			characterAmount = pkt.GetInt();
			weight = pkt.GetChar();
			maxWeight = pkt.GetChar();

			hpGain = hp = tp = 0;
			hairColor = 0;
			effect = 0;

			curecurse_stats = null;
			expreward_stats = null;

			//format differs based on item type
			//(keeping this in order with how eoserv ITEM_USE handler is ordered
			switch (type)
			{
				case ItemType.Teleport: /*Warp packet handles the rest!*/ break;
				case ItemType.Heal:
					{
						hpGain = pkt.GetInt();
						hp = pkt.GetShort();
						tp = pkt.GetShort();
					}
					break;
				case ItemType.HairDye:
					{
						hairColor = pkt.GetChar();
					}
					break;
				case ItemType.Beer: /*No additional data*/ break;
				case ItemType.EffectPotion:
					{
						effect = pkt.GetShort();
					}
					break;
				case ItemType.CureCurse:
					{
						curecurse_stats = new CureCurseStats(pkt);
					}
					break;
				case ItemType.EXPReward:
					{
						//note: server packets may be incorrect at this point (src/handlers/Item.cpp) because of unused builder in eoserv
						//note: server also sends an ITEM_ACCEPT packet to surrounding players on level-up?
						expreward_stats = new LevelUpStats(pkt, true);
					}
					break;
			}
		}
	}

	public delegate void PlayerItemDropEvent(int characterAmount, byte weight, byte maxWeight, MapItem item);
	public delegate void RemoveMapItemEvent(short itemUID);
	public delegate void JunkItemEvent(short itemID, int numJunked, int numRemaining, byte weight, byte maxWeight);
	public delegate void GetItemEvent(short itemUID, short itemID, int amountTaken, byte weight, byte maxWeight);
	public delegate void UseItemEvent(ItemUseData data);
	public delegate void ItemChangeEvent(bool newItemObtained, short id, int amount, byte weight);

	partial class PacketAPI
	{
		/// <summary>
		/// Occurs when any player drops an item - if characterAmount == -1, this means the item was dropped by a player other than MainPlayer
		/// </summary>
		public event PlayerItemDropEvent OnDropItem;
		public event RemoveMapItemEvent OnRemoveItemFromMap;
		public event JunkItemEvent OnJunkItem;
		public event GetItemEvent OnGetItemFromMap;
		public event UseItemEvent OnUseItem;
		public event ItemChangeEvent OnItemChange;

		private void _createItemMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Drop), _handleItemDrop, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Add), _handleItemAdd, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Remove), _handleItemRemove, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Junk), _handleItemJunk, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Get), _handleItemGet, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Reply), _handleItemReply, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Obtain), _handleItemObtain, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Item, PacketAction.Kick), _handleItemKick, true);
			//todo: handle ITEM_ACCEPT (ExpReward item type) (I think it shows the level up animation?)
		}

		/// <summary>
		/// Pick up the item with the specified UID
		/// </summary>
		public bool GetItem(short uid)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Item, PacketAction.Get);
			pkt.AddShort(uid);

			return m_client.SendPacket(pkt);
		}

		public bool DropItem(short id, int amount, byte x = 255, byte y = 255) //255 means use character's current location
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Item, PacketAction.Drop);
			pkt.AddShort(id);
			pkt.AddInt(amount);
			if (x == 255 && y == 255)
			{
				pkt.AddByte(x);
				pkt.AddByte(y);
			}
			else
			{
				pkt.AddChar(x);
				pkt.AddChar(y);
			}

			return m_client.SendPacket(pkt);
		}

		public bool JunkItem(short id, int amount)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Item, PacketAction.Junk);
			pkt.AddShort(id);
			pkt.AddInt(amount);

			return m_client.SendPacket(pkt);
		}

		public bool UseItem(short itemID)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Item, PacketAction.Use);
			pkt.AddShort(itemID);

			return m_client.SendPacket(pkt);
		}
		
		private void _handleItemDrop(Packet pkt)
		{
			if (OnDropItem == null) return;
			short _id = pkt.GetShort();
			int _amount = pkt.GetThree();
			int characterAmount = pkt.GetInt(); //amount remaining for the character
			MapItem item = new MapItem
			{
				id = _id,
				amount = _amount,
				uid = pkt.GetShort(),
				x = pkt.GetChar(),
				y = pkt.GetChar(),
				//turn off drop protection since main player dropped it
				time = DateTime.Now.AddSeconds(-5),
				npcDrop = false,
				playerID = 0 //id of 0 means the currently logged in player owns it
			};
			byte characterWeight = pkt.GetChar(), characterMaxWeight = pkt.GetChar(); //character adjusted weights
			
			OnDropItem(characterAmount, characterWeight, characterMaxWeight, item);
		}

		private void _handleItemAdd(Packet pkt)
		{
			if (OnDropItem == null) return;
			MapItem item = new MapItem
			{
				id = pkt.GetShort(),
				uid = pkt.GetShort(),
				amount = pkt.GetThree(),
				x = pkt.GetChar(),
				y = pkt.GetChar(),
				time = DateTime.Now,
				npcDrop = false,
				playerID = -1 //another player dropped. drop protection says "Item protected" w/o player name
			};
			OnDropItem(-1, 0, 0, item);
		}

		private void _handleItemRemove(Packet pkt)
		{
			if (OnRemoveItemFromMap != null)
				OnRemoveItemFromMap(pkt.GetShort());
		}

		private void _handleItemJunk(Packet pkt)
		{
			short id = pkt.GetShort();
			int amountRemoved = pkt.GetThree();//don't really care - just math it
			int amountRemaining = pkt.GetInt();
			byte weight = pkt.GetChar();
			byte maxWeight = pkt.GetChar();

			if (OnJunkItem != null)
				OnJunkItem(id, amountRemoved, amountRemaining, weight, maxWeight);
		}

		private void _handleItemGet(Packet pkt)
		{
			if (OnGetItemFromMap == null) return;
			short uid = pkt.GetShort();
			short id = pkt.GetShort();
			int amountTaken = pkt.GetThree();
			byte weight = pkt.GetChar();
			byte maxWeight = pkt.GetChar();
			OnGetItemFromMap(uid, id, amountTaken, weight, maxWeight);
		}

		private void _handleItemReply(Packet pkt)
		{
			if (OnUseItem != null)
				OnUseItem(new ItemUseData(pkt));
		}

		private void _handleItemObtain(Packet pkt)
		{
			if (OnItemChange == null) return;

			short id = pkt.GetShort();
			int amount = pkt.GetThree();
			byte weight = pkt.GetChar();

			OnItemChange(true, id, amount, weight);
		}

		private void _handleItemKick(Packet pkt)
		{
			if (OnItemChange == null) return;

			short id = pkt.GetShort();
			int amount = pkt.GetThree();
			byte weight = pkt.GetChar();

			OnItemChange(false, id, amount, weight);
		}
	}
}
