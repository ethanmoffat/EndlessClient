using System;

namespace EOLib.Net
{
	partial class PacketAPI
	{
		public delegate void PlayerAttackEvent(short playerID, EODirection dir);
		public event PlayerAttackEvent OnOtherPlayerAttack;

		private void _createAttackMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Attack, PacketAction.Player), _handleAttackPlayer, true);
		}

		public bool AttackUse(EODirection direction)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized) return false;

			Packet pkt = new Packet(PacketFamily.Attack, PacketAction.Use);
			pkt.AddChar((byte)direction);
			pkt.AddThree(DateTime.Now.ToEOTimeStamp());

			return m_client.SendPacket(pkt);
		}

		/// <summary>
		/// Sent when another player attacks (not main player)
		/// </summary>
		private void _handleAttackPlayer(Packet pkt)
		{
			if (OnOtherPlayerAttack == null) return;
			short playerId = pkt.GetShort();
			EODirection dir = (EODirection)pkt.GetChar();
			OnOtherPlayerAttack(playerId, dir);
		}
	}
}
