namespace EOLib.Net
{
	internal enum TrainType
	{
		Stat = 1,
		Skill = 2
	}

	partial class PacketAPI
	{
		private void _createStatSkillMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Player), _handleStatSkillPlayer, true);
		}

		public bool AddStatPoint(short statID)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.StatSkill, PacketAction.Add);
			pkt.AddChar((byte)TrainType.Stat);
			pkt.AddShort(statID);

			return m_client.SendPacket(pkt);
		}

		private void _handleStatSkillPlayer(Packet pkt)
		{
			if (OnStatsList != null)
				OnStatsList(new DisplayStats(pkt, true));
		}
	}
}
