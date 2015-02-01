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

		/// <summary>
		/// Sends PLAYERS_ACCEPT packet which is sent for the #find command
		/// </summary>
		/// <param name="characterName">Name of the character to find</param>
		public static void Find(string characterName)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return;

			Packet pkt = new Packet(PacketFamily.Players, PacketAction.Accept);
			pkt.AddString(characterName);
			if(!client.SendPacket(pkt))
				EOGame.Instance.LostConnectionDialog();
		}

		/// <summary>
		/// Handles PLAYERS_PING packet which is sent in response to the #find command
		/// </summary>
		public static void PlayersPing(Packet pkt)
		{
			string charName = pkt.GetEndString();
			if (charName.Length == 0) return;

			EOGame.Instance.Hud.AddChat(ChatTabs.Local,
				"System", 
				string.Format("{0} was nowhere to be found.", char.ToUpper(charName[0]) + charName.Substring(1)), 
				ChatType.LookingDude);
		}

		/// <summary>
		/// Handles PLAYERS_PONG packet which is sent in response to the #find command
		/// </summary>
		public static void PlayersPong(Packet pkt)
		{
			string charName = pkt.GetEndString();
			if (charName.Length == 0) return;

			EOGame.Instance.Hud.AddChat(ChatTabs.Local,
				"System",
				string.Format("{0} is online and on the same map.", char.ToUpper(charName[0]) + charName.Substring(1)),
				ChatType.LookingDude);
		}

		/// <summary>
		/// Handles PLAYERS_NET3 packet which is sent in response to the #find command
		/// </summary>
		public static void PlayersNet3(Packet pkt)
		{
			string charName = pkt.GetEndString();
			if (charName.Length == 0) return;

			EOGame.Instance.Hud.AddChat(ChatTabs.Local,
				"System",
				string.Format("{0} is online somewhere in this world.", char.ToUpper(charName[0]) + charName.Substring(1)),
				ChatType.LookingDude);
		}
	}
}
