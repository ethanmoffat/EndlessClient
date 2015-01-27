using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EOLib;

namespace EndlessClient.Handlers
{
	public static class Attack
	{
		public static bool AttackUse(EODirection direction)
		{
			EOClient client = (EOClient) World.Instance.Client;
			if (!client.ConnectedAndInitialized) return false;

			Packet pkt = new Packet(PacketFamily.Attack, PacketAction.Use);
			pkt.AddChar((byte)direction);
			pkt.AddThree(DateTime.Now.ToEOTimeStamp());

			return client.SendPacket(pkt);
		}

		/// <summary>
		/// Sent when another player attacks (not main player)
		/// </summary>
		public static void AttackPlayerResponse(Packet pkt)
		{
			short playerId = pkt.GetShort();
			EODirection dir = (EODirection)pkt.GetChar();
			World.Instance.ActiveMapRenderer.OtherPlayerAttack(playerId, dir);
		}
	}
}
