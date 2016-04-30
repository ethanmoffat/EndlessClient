// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
	public enum Emote
	{
		Happy = 1,
		Depressed = 2,
		Sad = 3,
		Angry = 4,
		Confused = 5,
		Surprised = 6,
		Hearts = 7,
		Moon = 8,
		Suicidal = 9,
		/// <summary>
		/// DEL or . key
		/// </summary>
		Embarassed = 10,
		Drunk = 11,
		Trade = 12,
		LevelUp = 13,
		/// <summary>
		/// 0 key
		/// </summary>
		Playful = 14
	}

	partial class PacketAPI
	{
		public event Action<short, Emote> OnOtherPlayerEmote;

		private void _createEmoteMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Emote, PacketAction.Player), _handleEmotePlayer, true);
		}

		public bool ReportEmote(Emote emote)
		{
			//trade/level up happen differently
			if (emote == Emote.Trade || emote == Emote.LevelUp)
				return false; //signal error client-side

			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			OldPacket pkt = new OldPacket(PacketFamily.Emote, PacketAction.Report);
			pkt.AddChar((byte)emote);

			return m_client.SendPacket(pkt);
		}

		private void _handleEmotePlayer(OldPacket pkt)
		{
			short playerID = pkt.GetShort();
			Emote emote = (Emote)pkt.GetChar();

			if(OnOtherPlayerEmote != null)
				OnOtherPlayerEmote(playerID, emote);
		}
	}
}
