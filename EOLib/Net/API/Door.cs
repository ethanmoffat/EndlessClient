// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Net.API
{
	partial class PacketAPI
	{
		public event Action<byte, byte> OnDoorOpen;

		private void _createDoorMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Door, PacketAction.Open), _handleDoorOpen, true);
		}

		public bool DoorOpen(byte x, byte y)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized) return false;

			Packet builder = new Packet(PacketFamily.Door, PacketAction.Open);
			builder.AddChar(x);
			builder.AddChar(y);

			return m_client.SendPacket(builder);
		}

		//also a DOOR_OPEN packet
		private void _handleDoorOpen(Packet pkt)
		{
			if (OnDoorOpen == null) return;

			//returns: x, y (char, short) of door location - 
			//  but the short (y) is expanded from a byte server-side so no loss of data
			OnDoorOpen(pkt.GetChar(), (byte)pkt.GetShort());
		}
	}
}
