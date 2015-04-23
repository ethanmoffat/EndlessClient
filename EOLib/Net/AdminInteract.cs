using System;

namespace EOLib.Net
{
	partial class PacketAPI
	{
		/// <summary>
		/// Occurs when the hidden state of an admin changes. Includes short PlayerID and bool Hidden (true if player is hiding, false if visible).
		/// </summary>
		public event Action<short, bool> OnAdminHiddenChange;

		private void _createAdminInteractMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.AdminInteract, PacketAction.Remove), _handleAdminHide, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.AdminInteract, PacketAction.Agree),  _handleAdminShow, true);
		}

		private void _handleAdminHide(Packet pkt)
		{
			if (OnAdminHiddenChange == null) return;
			short id = pkt.GetShort();

			OnAdminHiddenChange(id, true);
		}

		private void _handleAdminShow(Packet pkt)
		{
			if (OnAdminHiddenChange == null) return;
			short id = pkt.GetShort();

			OnAdminHiddenChange(id, false);
		}
	}
}
