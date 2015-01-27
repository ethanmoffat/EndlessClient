using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EOLib;

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
					bool isDead = pkt.GetChar() == 2; //2 if dead, 1 if alive
					EODirection dir = (EODirection) pkt.GetChar();
					short targetPlayerID = pkt.GetShort();
					int damage = pkt.GetThree();
					int pctHealth = pkt.GetThree(); //percentage of health remaining
					if (pkt.GetByte() != 255 || pkt.GetByte() != 255)
						return;
					//todo: do the attack (via map renderer)
				}
					break;
				case 2: /*npc talk!*/
				{
					short index = pkt.GetShort();
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
			short playerID = pkt.GetShort();
			byte direction = pkt.GetChar();
			short deadNPC = pkt.GetShort();

			World.Instance.ActiveMapRenderer.RemoveOtherNPC((byte)deadNPC, pkt.ReadPos < pkt.Length);
			if (pkt.ReadPos == pkt.Length) return;

			short droppedItemUID = pkt.GetShort();
			short droppedItemID = pkt.GetShort();
			byte x = pkt.GetChar();
			byte y = pkt.GetChar();
			int droppedAmount = pkt.GetInt();
			int damage = pkt.GetThree();
			if (droppedItemID > 0)
			{
				World.Instance.ActiveMapRenderer.MapItems.Add(new MapItem
				{
					amount = droppedAmount,
					id = droppedItemID,
					uid = droppedItemUID,
					x = x,
					y = y
				});
			}
		}

		/// <summary>
		/// Handler for NPC_REPLY packet, when NPC takes damage from an attack (spell cast or weapon) but is still alive
		/// </summary>
		public static void NPCReply(Packet pkt)
		{
			
		}
	}
}
