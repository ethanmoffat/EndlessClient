using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Effects;
using EOLib;

namespace EndlessClient.Handlers
{
	public static class Emote
	{
		public static bool ReportEmote(EndlessClient.Emote emote)
		{
			//trade/level up happen differently
			if (emote == EndlessClient.Emote.Trade || emote == EndlessClient.Emote.LevelUp)
				return false; //signal error client-side

			EOClient client = (EOClient) World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Emote, PacketAction.Report);
			pkt.AddChar((byte)emote);

			return client.SendPacket(pkt);
		}

		public static void EmotePlayer(Packet pkt)
		{
			short playerID = pkt.GetShort();
			EndlessClient.Emote emote = (EndlessClient.Emote) pkt.GetChar();

			if(playerID != World.Instance.MainPlayer.ActiveCharacter.ID)
				World.Instance.ActiveMapRenderer.OtherPlayerEmote(playerID, emote);
		}
	}
}
