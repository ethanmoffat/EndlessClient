using EOLib;
using EOLib.Net;

namespace EndlessClient.Handlers
{
	public static class Face
	{
		/// <summary>
		/// Changes the direction of the player server-side
		/// </summary>
		public static bool FacePlayer(EODirection dir)
		{
			Packet pkt = new Packet(PacketFamily.Face, PacketAction.Player);
			pkt.AddChar((byte)dir);
			EOClient client = (EOClient)World.Instance.Client;

			return client != null && client.ConnectedAndInitialized && client.SendPacket(pkt);
		}

		public static void FacePlayerResponse(Packet pkt)
		{
			short playerId = pkt.GetShort();
			EODirection dir = (EODirection)pkt.GetChar();

			World.Instance.ActiveMapRenderer.OtherPlayerFace(playerId, dir);
		}
	}
}
