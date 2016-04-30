// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
	partial class PacketAPI
	{
		private Dictionary<short, DateTime> requests;
		private Random gen;

		public event Action<int> OnServerPingReply;
		public event Action<string> OnStatusMessage;

		private void _createMessageMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Message, PacketAction.Pong), _handleMessagePong, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Message, PacketAction.Open), _handleMessageOpen, true);
		}

		public bool PingServer()
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			if(requests == null)
				 requests = new Dictionary<short, DateTime>();
			if(gen == null)
				gen = new Random();

			short nextReq;
			do
			{
				nextReq = (short)gen.Next(ushort.MinValue, short.MaxValue - 1);
			} while (requests.ContainsKey(nextReq));

			OldPacket pkt = new OldPacket(PacketFamily.Message, PacketAction.Ping);
			pkt.AddShort(nextReq);

			requests.Add(nextReq, DateTime.Now);
			return m_client.SendPacket(pkt);
		}

		private void _handleMessagePong(OldPacket pkt)
		{
			DateTime now = DateTime.Now;
			short req = pkt.GetShort();
			if (requests == null || !requests.ContainsKey(req))
				return;

			int ms = (int) (now - requests[req]).TotalMilliseconds;
			requests.Remove(req);

			if (OnServerPingReply != null)
				OnServerPingReply(ms);
		}

		private void _handleMessageOpen(OldPacket pkt)
		{
			if (OnStatusMessage != null)
				OnStatusMessage(pkt.GetEndString());
		}
	}
}
