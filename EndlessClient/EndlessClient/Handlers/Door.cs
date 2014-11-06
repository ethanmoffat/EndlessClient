using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EOLib;

namespace EndlessClient.Handlers
{
	public static class Door
	{
		public static void DoorOpen(byte x, byte y)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized) return;

			Packet builder = new Packet(PacketFamily.Door, PacketAction.Open);
			builder.AddChar(x);
			builder.AddChar(y);

			client.SendPacket(builder);
		}

		//also a DOOR_OPEN packet
		public static void DoorOpenResponse(Packet pkt)
		{
			//returns: x, y (char, short) of door location
			World.Instance.ActiveMapRenderer.OpenDoor(pkt.GetChar(), pkt.GetShort());
		}
	}
}
