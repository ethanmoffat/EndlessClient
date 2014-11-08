using EOLib;

namespace EndlessClient.Handlers
{
	public static class Players
	{
		/// <summary>
		/// Handles PLAYERS_AGREE packet which is sent when a player enters a map by warp or upon spawning
		/// </summary>
		public static void PlayersAgree(Packet pkt)
		{
			if (pkt.GetByte() != 255)
				return;

			EndlessClient.Character newGuy = new EndlessClient.Character(pkt);

			byte nextByte = pkt.GetByte();
			WarpAnimation anim = WarpAnimation.None;
			if (nextByte != 255) //next byte was the warp animation: sent on Map::Enter in eoserv
			{
				pkt.Skip(-1);
				anim = (WarpAnimation) pkt.GetChar();
				if (pkt.GetByte() != 255) //the 255 still needs to be read...
					return;
			}
			//else... //next byte was a 255. proceed normally.

			if (pkt.GetChar() == 1) //0 for NPC, 1 for player. In eoserv it is never 0.
				World.Instance.ActiveMapRenderer.AddOtherPlayer(newGuy, anim);
		}
	}
}
