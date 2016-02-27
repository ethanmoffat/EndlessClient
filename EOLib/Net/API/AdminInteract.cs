// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Net.API
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

		private void _handleAdminHide(OldPacket pkt)
		{
			if (OnAdminHiddenChange == null) return;
			short id = pkt.GetShort();

			OnAdminHiddenChange(id, true);
		}

		private void _handleAdminShow(OldPacket pkt)
		{
			if (OnAdminHiddenChange == null) return;
			short id = pkt.GetShort();

			OnAdminHiddenChange(id, false);
		}
	}
}
