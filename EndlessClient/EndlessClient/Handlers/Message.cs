using System;
using System.Collections.Generic;
using EOLib;
using EOLib.Net;

namespace EndlessClient.Handlers
{
	public static class Message
	{
		private static Dictionary<short, DateTime> requests;
		private static Random gen;

		public static void Ping()
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return;

			if(requests == null)
				 requests = new Dictionary<short, DateTime>();
			if(gen == null)
				gen = new Random();

			short nextReq;
			do
			{
				nextReq = (short)gen.Next(ushort.MinValue, short.MaxValue - 1);
			} while (requests.ContainsKey(nextReq));

			Packet pkt = new Packet(PacketFamily.Message, PacketAction.Ping);
			pkt.AddShort(nextReq);

			requests.Add(nextReq, DateTime.Now);
			client.SendPacket(pkt);
		}

		public static void Pong(Packet pkt)
		{
			DateTime now = DateTime.Now;
			short req = pkt.GetShort();
			if (requests == null || !requests.ContainsKey(req))
				return;

			int ms = (int)(now - requests[req]).TotalMilliseconds;
			EOGame.Instance.Hud.AddChat(ChatTabs.Local, "System", string.Format("[x] Current ping to the server is: {0} ms.", ms), ChatType.LookingDude);
			requests.Remove(req);
		}
	}
}
