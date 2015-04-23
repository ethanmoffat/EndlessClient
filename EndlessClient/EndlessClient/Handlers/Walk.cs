
using System;
using EOLib;
using EOLib.Net;

namespace EndlessClient.Handlers
{
	public static class Walk
	{
		/// <summary>
		/// Sends a walk packet to the server (ActiveCharacter did a walk)
		/// <para>This is being done as fire and forget so the game doesn't lock up</para>
		/// </summary>
		public static bool PlayerWalk(EODirection dir, byte destX, byte destY, bool admin = false)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized)
				return false;

			Packet builder = new Packet(PacketFamily.Walk, admin ? PacketAction.Admin : PacketAction.Player); //change family/action
			builder.AddChar((byte)dir);
			builder.AddThree(DateTime.Now.ToEOTimeStamp());
			builder.AddChar(destX);
			builder.AddChar(destY);

			return client.SendPacket(builder);
		}

		/// <summary>
		/// Sent in response to a character request to walk (ActiveCharacter)
		/// </summary>
		public static void WalkReply(Packet pkt)
		{
			if (pkt.GetByte() != 255 || pkt.GetByte() != 255)
				return;//something is off
			//response contains the map items that are now in range
			int numberOfMapItems = pkt.PeekEndString().Length/9;
			for (int i = 0; i < numberOfMapItems; ++i)
			{
				World.Instance.ActiveMapRenderer.AddMapItem(new MapItem
				{
					uid = pkt.GetShort(),
					id = pkt.GetShort(),
					x = pkt.GetChar(),
					y = pkt.GetChar(),
					amount = pkt.GetThree()
				});
			}
		}

		/// <summary>
		/// Sent when another character walks
		/// </summary>
		public static void WalkPlayer(Packet pkt)
		{
			short playerID = pkt.GetShort();
			EODirection dir = (EODirection) pkt.GetChar();
			byte x = pkt.GetChar();
			byte y = pkt.GetChar();
			World.Instance.ActiveMapRenderer.OtherPlayerWalk(playerID, dir, x, y);
		}

		public static void Cleanup()
		{
			//response.Dispose();
		}
	}
}
