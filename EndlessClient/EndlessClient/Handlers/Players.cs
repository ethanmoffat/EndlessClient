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

			WarpAnimation anim = (WarpAnimation)pkt.GetChar();
			if (pkt.GetByte() != 255)
				return;
			if (pkt.GetChar() == 1) //0 for NPC, 1 for player
				World.Instance.ActiveMapRenderer.AddOtherPlayer(newGuy, anim);
			else
			{
				//add other npc
			}
		}
	}
}
