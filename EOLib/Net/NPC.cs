using System;

namespace EOLib.Net
{
	public struct NPCData
	{
		private readonly byte m_index, m_x, m_y;
		private readonly EODirection m_dir;
		private readonly short m_id;

		public byte Index { get { return m_index; } }
		public short ID { get { return m_id; } }
		public byte X { get { return m_x; } }
		public byte Y { get { return m_y; } }
		public EODirection Direction { get { return m_dir; } }

		internal NPCData(Packet pkt)
		{
			m_index = pkt.GetChar();
			m_id = pkt.GetShort();
			m_x = pkt.GetChar();
			m_y = pkt.GetChar();
			m_dir = (EODirection)pkt.GetChar();
		}
	}

	public delegate void NPCWalkEvent(byte index, byte x, byte y, EODirection dir);
	public delegate void NPCAttackEvent(byte index, bool targetPlayerIsDead, EODirection dir, short targetPlayerID, int damage, int percentHealth);
	public delegate void NPCChatEvent(byte index, string message);
	public delegate void NPCLeaveMapEvent(byte index, int damageToNPC, short playerID, EODirection playerDirection, short tpRemaining = -1, short spellID = -1);
	public delegate void NPCKilledEvent(int exp);
	public delegate void NPCTakeDamageEvent(byte npcIndex, short fromPlayerID, EODirection fromDirection, int damageToNPC, int npcPctHealth, short spellID = -1, short fromTP = -1);

	partial class PacketAPI
	{
		public event Action<NPCData> OnNPCEnterMap;
		public event NPCWalkEvent OnNPCWalk;
		public event NPCAttackEvent OnNPCAttack;
		public event NPCChatEvent OnNPCChat;
		public event NPCLeaveMapEvent OnNPCLeaveMap;
		public event NPCKilledEvent OnNPCKilled; //int is the experience gained
		public event NPCTakeDamageEvent OnNPCTakeDamage;
		public event Action<LevelUpStats> OnPlayerLevelUp;
		public event Action<short> OnRemoveChildNPCs;

		private void _createNPCMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Appear, PacketAction.Reply), _handleAppearReply, true);

			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.NPC, PacketAction.Accept), _handleNPCAccept, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Cast, PacketAction.Accept), _handleNPCAccept, true);

			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.NPC, PacketAction.Player), _handleNPCPlayer, true);

			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.NPC, PacketAction.Reply), _handleNPCReply, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Cast, PacketAction.Reply), _handleNPCReply, true);

			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.NPC, PacketAction.Spec), _handleNPCSpec, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Cast, PacketAction.Spec), _handleNPCSpec, true);

			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.NPC, PacketAction.Junk), _handleNPCJunk, true);
		}

		private void _handleAppearReply(Packet pkt)
		{
			if (pkt.Length - pkt.ReadPos != 8 || 
				pkt.GetChar() != 0 || pkt.GetByte() != 255 ||
				OnNPCEnterMap == null)
				return; //malformed packet

			OnNPCEnterMap(new NPCData(pkt));
		}

		/// <summary>
		/// Handler for NPC_PLAYER packet, when NPC walks or talks
		/// </summary>
		private void _handleNPCPlayer(Packet pkt)
		{
			int num255s = 0;
			while (pkt.PeekByte() == 255)
			{
				num255s++;
				pkt.GetByte();
			}

			switch (num255s)
			{
				case 0: /*npc walk!*/
					{
						//npc remove from view sets x/y to either 0,0 or 252,252 based on target coords
						byte index = pkt.GetChar();
						byte x = pkt.GetChar(), y = pkt.GetChar();
						EODirection dir = (EODirection)pkt.GetChar();
						if (pkt.GetByte() != 255 || pkt.GetByte() != 255 || pkt.GetByte() != 255 || OnNPCWalk == null)
							return;
						OnNPCWalk(index, x, y, dir);
					}
					break;
				case 1: /*npc attack!*/
					{
						byte index = pkt.GetChar();
						bool isDead = pkt.GetChar() == 2; //2 if target player is dead, 1 if alive
						EODirection dir = (EODirection)pkt.GetChar(); //NPC direction
						short targetPlayerID = pkt.GetShort();
						int damage = pkt.GetThree(); //damage done to player
						int pctHealth = pkt.GetThree(); //percentage of health remaining of target player
						if (pkt.GetByte() != 255 || pkt.GetByte() != 255 || OnNPCAttack == null)
							return;
						OnNPCAttack(index, isDead, dir, targetPlayerID, damage, pctHealth);
					}
					break;
				case 2: /*npc talk!*/
					{
						byte index = pkt.GetChar();
						byte msgLength = pkt.GetChar();
						string msg = pkt.GetFixedString(msgLength);
						if (OnNPCChat == null) return;
						OnNPCChat(index, msg);
					}
					break;
			}
		}

		/// <summary>
		/// Handler for NPC_SPEC packet, when NPC should be removed from view - either by dying or out of character range
		/// </summary>
		private void _handleNPCSpec(Packet pkt)
		{
			short spellID = -1;
			if (pkt.Family == PacketFamily.Cast)
				spellID = pkt.GetShort();

			short playerID = pkt.GetShort(); //player that is protecting the item
			EODirection playerDirection = (EODirection)pkt.GetChar();
			short deadNPCIndex = pkt.GetShort();

			if (pkt.ReadPos == pkt.Length)
			{
				if (OnNPCLeaveMap != null)
					OnNPCLeaveMap((byte) deadNPCIndex, 0, playerID, playerDirection);
				return; //just removing from range, packet ends here
			}

			short droppedItemUID = pkt.GetShort();
			short droppedItemID = pkt.GetShort();
			byte x = pkt.GetChar();
			byte y = pkt.GetChar();
			int droppedAmount = pkt.GetInt();
			int damageDoneToNPC = pkt.GetThree();

			short characterTPRemaining = -1;
			if (pkt.Family == PacketFamily.Cast)
				characterTPRemaining = pkt.GetShort();

			if(OnNPCLeaveMap != null)
				OnNPCLeaveMap((byte)deadNPCIndex, damageDoneToNPC, playerID, playerDirection, characterTPRemaining, spellID);

			//just showing a dropped item, packet ends here
			if (pkt.ReadPos == pkt.Length)
			{
				_showDroppedItemIfNeeded(playerID, droppedItemID, droppedAmount, droppedItemUID, x, y);
				return;
			}

			int newExp = pkt.GetInt(); //npc was killed - this handler was invoked from NPCAccept
			if (OnNPCKilled != null)
				OnNPCKilled(newExp);

			//the order in the original client is: 'you gained {x} EXP' and then 'The NPC dropped {x}'
			//Otherwise, I would just do the drop item logic once above
			_showDroppedItemIfNeeded(playerID, droppedItemID, droppedAmount, droppedItemUID, x, y);
		}

		private void _showDroppedItemIfNeeded(short playerID, short droppedItemID, int droppedAmount, short droppedItemUID, byte x, byte y)
		{
			if (droppedItemID > 0 && OnDropItem != null)
			{
				OnDropItem(-1, 0, 0, new MapItem
				{
					amount = droppedAmount,
					id = droppedItemID,
					uid = droppedItemUID,
					x = x,
					y = y,
					time = DateTime.Now,
					npcDrop = true,
					playerID = playerID
				});
			}
		}

		/// <summary>
		/// Handler for NPC_REPLY packet, when NPC takes damage from an attack (spell cast or weapon) but is still alive
		/// </summary>
		private void _handleNPCReply(Packet pkt)
		{
			if (OnNPCTakeDamage == null) return;

			short spellID = -1;
			if (pkt.Family == PacketFamily.Cast)
				spellID = pkt.GetShort();

			short fromPlayerID = pkt.GetShort();
			EODirection fromDirection = (EODirection)pkt.GetChar();
			short npcIndex = pkt.GetShort();
			int damageToNPC = pkt.GetThree();
			int npcPctHealth = pkt.GetShort();

			short fromTP = -1;
			if (pkt.Family == PacketFamily.Cast)
				fromTP = pkt.GetShort();
			else if (pkt.GetChar() != 1) //some constant 1 in EOSERV
				return;

			OnNPCTakeDamage((byte)npcIndex, fromPlayerID, fromDirection, damageToNPC, npcPctHealth, spellID, fromTP);
		}

		/// <summary>
		/// Handler for NPC_ACCEPT packet, when character levels up from exp earned when killing an NPC
		/// </summary>
		private void _handleNPCAccept(Packet pkt)
		{
			_handleNPCSpec(pkt); //same handler for the first part of the packet

			if (OnPlayerLevelUp != null)
				OnPlayerLevelUp(new LevelUpStats(pkt, false));
		}

		private void _handleNPCJunk(Packet pkt)
		{
			if (OnRemoveChildNPCs != null)
				OnRemoveChildNPCs(pkt.GetShort());
		}
	}
}
