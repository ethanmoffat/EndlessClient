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
			foreach (EndlessClient.Character _c in newChars)
			{
				World.Instance.ActiveMapRenderer.AddOtherPlayer(_c);
			}

			World.Instance.ActiveMapRenderer.NPCs.Clear();
			while (pkt.PeekByte() != 255)
			{
				NPC newGuy = new NPC(pkt);
				World.Instance.ActiveMapRenderer.NPCs.Add(newGuy);
			}
			pkt.GetByte();

			World.Instance.ActiveMapRenderer.MapItems.Clear();
			while (pkt.ReadPos < pkt.Length)
			{
				World.Instance.ActiveMapRenderer.MapItems.Add(new MapItem
				{
					uid = pkt.GetShort(),
					id = pkt.GetShort(),
					x = pkt.GetByte(),
					y = pkt.GetByte(),
					amount = pkt.GetThree()
				});
			}
		}
	}
}
