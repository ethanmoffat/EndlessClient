namespace EOLib.Net
{
	partial class PacketAPI
	{
		public delegate void PlayerEnterMapEvent(CharacterData data, WarpAnimation anim = WarpAnimation.None);
		public delegate void PlayerFindCommandEvent(bool isOnline, bool sameMap, string name);

		public event PlayerEnterMapEvent OnPlayerEnterMap;

		/// <summary>
		/// Occurs when the server responds to a #find command
		/// </summary>
		public event PlayerFindCommandEvent OnPlayerFindCommandReply;

		private void _createPlayersMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Players, PacketAction.Agree), _handlePlayersAgree, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Players, PacketAction.Ping), _handlePlayersPing, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Players, PacketAction.Pong), _handlePlayersPong, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Players, PacketAction.Net3), _handlePlayersNet3, true);
		}

		public bool FindPlayer(string characterName)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Players, PacketAction.Accept);
			pkt.AddString(characterName);

			return m_client.SendPacket(pkt);
		}

		private void _handlePlayersPing(Packet pkt)
		{
			if (OnPlayerFindCommandReply != null)
				OnPlayerFindCommandReply(false, false, pkt.GetEndString());
		}
		private void _handlePlayersPong(Packet pkt)
		{
			if (OnPlayerFindCommandReply != null)
				OnPlayerFindCommandReply(true, true, pkt.GetEndString());
		}
		private void _handlePlayersNet3(Packet pkt)
		{
			if (OnPlayerFindCommandReply != null)
				OnPlayerFindCommandReply(true, false, pkt.GetEndString());
		}

		// Handles PLAYERS_AGREE packet which is sent when a player enters a map by warp or upon spawning
		private void _handlePlayersAgree(Packet pkt)
		{
			if (pkt.GetByte() != 255 || OnPlayerEnterMap == null)
				return;

			CharacterData newGuy = new CharacterData(pkt);

			byte nextByte = pkt.GetByte();
			WarpAnimation anim = WarpAnimation.None;
			if (nextByte != 255) //next byte was the warp animation: sent on Map::Enter in eoserv
			{
				pkt.Skip(-1);
				anim = (WarpAnimation)pkt.GetChar();
				if (pkt.GetByte() != 255) //the 255 still needs to be read...
					return;
			}
			//else... //next byte was a 255. proceed normally.

			if (pkt.GetChar() == 1) //0 for NPC, 1 for player. In eoserv it is never 0.
				OnPlayerEnterMap(newGuy, anim);
		}
	}
}
