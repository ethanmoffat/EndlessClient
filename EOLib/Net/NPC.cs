namespace EOLib.Net
{
	public struct NPCData
	{
		private readonly byte m_index, m_x, m_y;
		private readonly EODirection m_dir;
		private readonly short m_id;

		public byte Index { get { return m_index; } }
		public short ID { get { return m_id; } }
		public byte X { get { return m_x; } }
		public byte Y { get { return m_y; } }
		public EODirection Direction { get { return m_dir; } }

		internal NPCData(Packet pkt)
		{
			m_index = pkt.GetChar();
			m_id = pkt.GetShort();
			m_x = pkt.GetChar();
			m_y = pkt.GetChar();
			m_dir = (EODirection)pkt.GetChar();
		}
	}

	partial class PacketAPI
	{
		public delegate void NPCEnterMapEvent(NPCData data);

		public event NPCEnterMapEvent OnNPCEnterMap;

		private void _createNPCMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Appear, PacketAction.Reply), _handleAppearReply, true);
		}

		private void _handleAppearReply(Packet pkt)
		{
			if (pkt.Length - pkt.ReadPos != 8 || 
				pkt.GetChar() != 0 || pkt.GetByte() != 255 ||
				OnNPCEnterMap == null)
				return; //malformed packet

			OnNPCEnterMap(new NPCData(pkt));
		}
	}
}
