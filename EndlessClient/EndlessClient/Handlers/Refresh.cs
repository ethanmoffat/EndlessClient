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
			List<MapItem> newItems = new List<MapItem>();
			
			byte numOtherChars = pkt.GetChar();
			if (pkt.GetByte() != 255)
				return;

			for (int i = 0; i < numOtherChars; ++i)
			{
				EndlessClient.Character charr = new EndlessClient.Character(pkt);
				if (charr.ID == World.Instance.MainPlayer.ActiveCharacter.ID)
					World.Instance.MainPlayer.ActiveCharacter.ApplyData(charr);
				else
					World.Instance.ActiveMapRenderer.AddOtherPlayer(charr);
				if (pkt.GetByte() != 255)
					return;
			}

			while (pkt.PeekByte() != 255)
			{
				pkt.Skip(6);
				//todo: npcs
				//UTIL_FOREACH(updatenpcs, npc)
				//{
				//	builder.AddChar(npc->index);
				//	builder.AddShort(npc->Data().id);
				//	builder.AddChar(npc->x);
				//	builder.AddChar(npc->y);
				//	builder.AddChar(npc->direction);
				//}
			}
			pkt.GetByte();

			while (pkt.ReadPos != pkt.Length)
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
