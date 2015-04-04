using System;
using EOLib;
using EOLib.Data;

namespace EndlessClient.Handlers
{
	public static class NPCPackets
	{
		/// <summary>
		/// Handler for the APPEAR_REPLY packet, when an NPC comes into view
		/// </summary>
		public static void AppearReply(Packet pkt)
		{
			if (pkt.Length - pkt.ReadPos != 8 || pkt.GetChar() != 0 || pkt.GetByte() != 255) return; //malformed packet
			World.Instance.ActiveMapRenderer.AddOtherNPC(new NPC(pkt));
		}

		/// <summary>
		/// Handler for NPC_PLAYER packet, when NPC walks or talks
		/// </summary>
		public static void NPCPlayer(Packet pkt)
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
					EODirection dir = (EODirection) pkt.GetChar();
					if (pkt.GetByte() != 255 || pkt.GetByte() != 255 || pkt.GetByte() != 255)
						return;
					World.Instance.ActiveMapRenderer.NPCWalk(index, x, y, dir);
				}
					break;
				case 1: /*npc attack!*/
				{
					byte index = pkt.GetChar();
					bool isDead = pkt.GetChar() == 2; //2 if target player is dead, 1 if alive
					EODirection dir = (EODirection) pkt.GetChar(); //NPC direction
					short targetPlayerID = pkt.GetShort();
					int damage = pkt.GetThree(); //damage done to player
					int pctHealth = pkt.GetThree(); //percentage of health remaining of target player
					if (pkt.GetByte() != 255 || pkt.GetByte() != 255)
						return;
					World.Instance.ActiveMapRenderer.NPCAttack(index, isDead, dir, targetPlayerID, damage, pctHealth);
				}
					break;
				case 2: /*npc talk!*/
				{
					byte index = pkt.GetChar();
					byte msgLength = pkt.GetChar();
					string msg = pkt.GetFixedString(msgLength);
					World.Instance.ActiveMapRenderer.RenderChatMessage(TalkType.NPC, index, msg, ChatType.Note);
				}
					break;
			}
		}

		/// <summary>
		/// Handler for NPC_SPEC packet, when NPC should be removed from view - either by dying or out of character range
		/// </summary>
		public static void NPCSpec(Packet pkt)
		{
			short playerID = pkt.GetShort(); //player that is protecting the item
			/*byte direction = */pkt.GetChar();
			short deadNPC = pkt.GetShort();

			if (pkt.ReadPos == pkt.Length)
			{
				World.Instance.ActiveMapRenderer.RemoveOtherNPC((byte)deadNPC);
				return; //just removing from range, packet ends here
			}

			short droppedItemUID = pkt.GetShort();
			short droppedItemID = pkt.GetShort();
			byte x = pkt.GetChar();
			byte y = pkt.GetChar();
			int droppedAmount = pkt.GetInt();
			int damage = pkt.GetThree(); //damage done to NPC.
			World.Instance.ActiveMapRenderer.RemoveOtherNPC((byte)deadNPC, damage);
			if (droppedItemID > 0)
			{
				World.Instance.ActiveMapRenderer.AddMapItem(new MapItem
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

			if (pkt.ReadPos == pkt.Length) 
			{
				if (droppedItemID > 0)
				{
					ItemRecord rec = World.Instance.EIF.GetItemRecordByID(droppedItemID);
					EOGame.Instance.Hud.AddChat(ChatTabs.System, "",
						string.Format("{0} {1} {2}", World.GetString(DATCONST2.STATUS_LABEL_THE_NPC_DROPPED), droppedAmount, rec.Name),
						ChatType.DownArrow);
				}
				return; //just showing a dropped item, packet ends here
			}

			int newExp = pkt.GetInt(); //npc was killed - this handler was invoked from NPCAccept
			int expDif = newExp - World.Instance.MainPlayer.ActiveCharacter.Stats.exp;
			World.Instance.MainPlayer.ActiveCharacter.GainExp(expDif);
			EOGame.Instance.Hud.RefreshStats();

			EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, DATCONST2.STATUS_LABEL_YOU_GAINED_EXP, string.Format(" {0} EXP", expDif));
			EOGame.Instance.Hud.AddChat(ChatTabs.System, "", string.Format("{0} {1} EXP", World.GetString(DATCONST2.STATUS_LABEL_YOU_GAINED_EXP), expDif), ChatType.Star);
			if (droppedItemID > 0)
			{
				ItemRecord rec = World.Instance.EIF.GetItemRecordByID(droppedItemID);
				EOGame.Instance.Hud.AddChat(ChatTabs.System, "", string.Format("{0} {1} {2}", World.GetString(DATCONST2.STATUS_LABEL_THE_NPC_DROPPED), droppedAmount, rec.Name), ChatType.DownArrow);
			}
		}

		/// <summary>
		/// Handler for NPC_REPLY packet, when NPC takes damage from an attack (spell cast or weapon) but is still alive
		/// </summary>
		public static void NPCReply(Packet pkt)
		{
			short fromPlayerID = pkt.GetShort();
			EODirection fromDirection = (EODirection)pkt.GetChar();
			short npcIndex = pkt.GetShort();
			int damageToNPC = pkt.GetThree();
			int npcPctHealth = pkt.GetShort();
			if (pkt.GetChar() != 1)
				return;

			World.Instance.ActiveMapRenderer.NPCTakeDamage(npcIndex, fromPlayerID, fromDirection, damageToNPC, npcPctHealth);
		}

		/// <summary>
		/// Handler for NPC_ACCEPT packet, when character levels up from exp earned when killing an NPC
		/// </summary>
		/// <param name="pkt"></param>
		public static void NPCAccept(Packet pkt)
		{
			NPCSpec(pkt); //same handler for the first part of the packet

			byte level = World.Instance.MainPlayer.ActiveCharacter.RenderData.level = pkt.GetChar();
			short statpts = pkt.GetShort();
			short skillpts = pkt.GetShort();
			short maxHP = pkt.GetShort();
			short maxTP = pkt.GetShort();
			short maxSP = pkt.GetShort();

			World.Instance.MainPlayer.ActiveCharacter.Emote(EndlessClient.Emote.LevelUp);
			World.Instance.ActiveCharacterRenderer.PlayerEmote();

			//local reference for readability
			CharStatData stats = World.Instance.MainPlayer.ActiveCharacter.Stats;
			stats.level = level;
			stats.statpoints = statpts;
			stats.skillpoints = skillpts;
			stats.SetMaxHP(maxHP);
			stats.SetMaxTP(maxTP);
			stats.SetMaxSP(maxSP);
			EOGame.Instance.Hud.RefreshStats();
		}
	}
}
