using System.Collections.Generic;
using System.Linq;
using EOLib;
using EOLib.Data;

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

			string lastPart = World.Instance.DataFiles[World.Instance.Localized2].Data[(int) DATCONST2.STATUS_LABEL_IS_ONLINE_NOT_FOUND];

			EOGame.Instance.Hud.AddChat(ChatTabs.Local,
				"System", 
				string.Format("{0} " + lastPart, char.ToUpper(charName[0]) + charName.Substring(1)), 
				ChatType.LookingDude);
		}

		/// <summary>
		/// Handles PLAYERS_PONG packet which is sent in response to the #find command
		/// </summary>
		public static void PlayersPong(Packet pkt)
		{
			string charName = pkt.GetEndString();
			if (charName.Length == 0) return;

			string lastPart = World.Instance.DataFiles[World.Instance.Localized2].Data[(int)DATCONST2.STATUS_LABEL_IS_ONLINE_SAME_MAP];

			EOGame.Instance.Hud.AddChat(ChatTabs.Local,
				"System",
				string.Format("{0} " + lastPart, char.ToUpper(charName[0]) + charName.Substring(1)),
				ChatType.LookingDude);
		}

		/// <summary>
		/// Handles PLAYERS_NET3 packet which is sent in response to the #find command
		/// </summary>
		public static void PlayersNet3(Packet pkt)
		{
			string charName = pkt.GetEndString();
			if (charName.Length == 0) return;

			string lastPart = World.Instance.DataFiles[World.Instance.Localized2].Data[(int)DATCONST2.STATUS_LABEL_IS_ONLINE_IN_THIS_WORLD];

			EOGame.Instance.Hud.AddChat(ChatTabs.Local,
				"System",
				string.Format("{0} " + lastPart, char.ToUpper(charName[0]) + charName.Substring(1)),
				ChatType.LookingDude);
		}

		//this has nothing to do with init logic but uses an INIT packet family
		//handles when the server sends a list of currently online players to the client
		public static void HandlePlayerList(Packet pkt, bool isFriendList)
		{
			short numTotal = pkt.GetShort();
			if (pkt.GetByte() != 255)
				return;

			List<OnlineEntry> onlinePlayers = new List<OnlineEntry>();
			Dictionary<int, string> classCache = new Dictionary<int, string>();
			for (int i = 0; i < numTotal; ++i)
			{
				string name = pkt.GetBreakString();

				if (!isFriendList)
				{
					string title = pkt.GetBreakString();
					if (pkt.GetChar() != 0)
						return;
					PaperdollIconType iconType = (PaperdollIconType) pkt.GetChar();
					int clsId = pkt.GetChar();
					string clss;
					if (classCache.ContainsKey(clsId))
					{
						clss = classCache[clsId];
					}
					else
					{
						clss = ((ClassRecord)World.Instance.ECF.Data.Find(dat => ((ClassRecord)dat).ID == clsId)).Name;
						classCache.Add(clsId, clss);
					}
					string guild = pkt.GetBreakString();

					if (string.IsNullOrWhiteSpace(title))
						title = "-";
					if (string.IsNullOrWhiteSpace(clss))
						clss = "-";
					if (string.IsNullOrWhiteSpace(guild))
						guild = "-";

					name = char.ToUpper(name[0]) + name.Substring(1);
					title = char.ToUpper(title[0]) + title.Substring(1);

					onlinePlayers.Add(new OnlineEntry(name, title, guild, clss, iconType));
				}
				else
				{
					onlinePlayers.Add(new OnlineEntry(name, "", "", "", PaperdollIconType.Normal));
				}
			}

			if (!isFriendList)
			{
				EOGame.Instance.Hud.SetOnlineList(onlinePlayers);
			}
			else if(EOFriendIgnoreListDialog.Instance != null)
			{
				EOFriendIgnoreListDialog.SetActive(onlinePlayers.Select(x => x.Name).ToList());
			}
		}
	}
}
