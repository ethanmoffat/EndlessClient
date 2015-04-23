using EOLib;
using EOLib.Net;

namespace EndlessClient.Handlers
{
	public static class Connection
	{
		//handler for CONNECTION_PLAYER: 
		//	update the client sequence number and send a ping back to the server
		public static void PingResponse(Packet pkt)
		{
			short seq_1 = pkt.GetShort();
			byte seq_2 = pkt.GetChar();
			((EOClient)World.Instance.Client).UpdateSequence(seq_1 - seq_2);

			Packet reply = new Packet(PacketFamily.Connection, PacketAction.Ping);
			World.Instance.Client.SendPacket(reply);
		}
	}
}
