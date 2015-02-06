using System;
using System.Collections.Generic;
using EOLib;

namespace EndlessClient.Handlers
{
	public static class Refresh
	{
		/// <summary>
		/// Called on server in Character::Refresh
		/// Refresh received from a server - contains character info, item info, and npc info
		/// </summary>
		public static void RefreshReply(Packet pkt)
		{
			List<EndlessClient.Character> newChars = new List<EndlessClient.Character>();
			
			byte numOtherChars = pkt.GetChar();
			if (pkt.GetByte() != 255)
				return;

			for (int i = 0; i < numOtherChars; ++i)
			{
				EndlessClient.Character charr = new EndlessClient.Character(pkt);
				if (charr.ID == World.Instance.MainPlayer.ActiveCharacter.ID)
					World.Instance.MainPlayer.ActiveCharacter.ApplyData(charr);
				else
					newChars.Add(charr);
				if (pkt.GetByte() != 255)
					return;
			}

			World.Instance.ActiveMapRenderer.ClearOtherPlayers();
			World.Instance.ActiveMapRenderer.ClearOtherNPCs();

			foreach (EndlessClient.Character _c in newChars)
			{
				World.Instance.ActiveMapRenderer.AddOtherPlayer(_c);
			}

			while (pkt.PeekByte() != 255)
			{
				NPC newGuy = new NPC(pkt);
				World.Instance.ActiveMapRenderer.AddOtherNPC(newGuy);
			}
			pkt.GetByte();

			World.Instance.ActiveMapRenderer.ClearMapItems();
			while (pkt.ReadPos < pkt.Length)
			{
				World.Instance.ActiveMapRenderer.AddMapItem(new MapItem
				{
					uid = pkt.GetShort(),
					id = pkt.GetShort(),
					x = pkt.GetByte(),
					y = pkt.GetByte(),
					amount = pkt.GetThree(),
					//turn off drop protection for items coming into view - server will validate
					time = DateTime.Now.AddSeconds(-5),
					npcDrop = false,
					playerID = -1
				});
			}
		}
	}
}
