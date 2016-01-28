// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.IO;

namespace EOLib.Net.API
{
	partial class PacketAPI
	{
		public event Action<short, EODirection> OnPlayerFace;

		private void _createFaceMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Face, PacketAction.Player), _handleFacePlayer, true);
		}

		/// <summary>
		/// Change the direction of the currently logged in player
		/// </summary>
		/// <param name="dir">Direction to face the currently logged in player</param>
		/// <returns>True on successful send, false otherwise</returns>
		public bool FacePlayer(EODirection dir)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Face, PacketAction.Player);
			pkt.AddChar((byte)dir);

			return m_client.SendPacket(pkt);
		}

		private void _handleFacePlayer(Packet pkt)
		{
			short playerId = pkt.GetShort();
			EODirection dir = (EODirection)pkt.GetChar();

			if (OnPlayerFace != null)
				OnPlayerFace(playerId, dir);
		}
	}
}
