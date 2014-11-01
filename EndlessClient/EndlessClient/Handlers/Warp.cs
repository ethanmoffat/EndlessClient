
using System.Collections.Generic;
using System.Threading;
using EOLib;

namespace EndlessClient.Handlers
{
	enum WarpReply
	{
		WarpLocal = 1,
		WarpSwitch = 2
	}

	//server sends WARP_REQUEST -> client sends WARP_ACCEPT -> server sends back WARP_AGREE
	//client sends WARP_TAKE (immediately before WARP_ACCEPT) if it needs a map
	public static class Warp
	{
		public static short RequestedMap { get; set; }
		/// <summary>
		/// Sent by server when ActiveCharacter should warp
		/// </summary>
		public static void WarpRequest(Packet pkt)
		{
			RequestedMap = -1;
			WarpReply warpType = (WarpReply)pkt.GetChar();
			switch (warpType)
			{
				case WarpReply.WarpLocal: WarpAccept(pkt.GetShort()); break; //pkt.GetChar() x2 for x,y coords
				case WarpReply.WarpSwitch:
					short mapID = pkt.GetShort();
					byte[] mapRid = pkt.GetBytes(4);
					int fileSize = pkt.GetThree();
					World.Instance.CheckMap(mapID, mapRid, fileSize);
					if (World.Instance.NeedMap == mapID)
					{
						RequestedMap = mapID;
						Init.WarpGetMap(); //does the WARP_TAKE if missing the map we need
					}
					WarpAccept(mapID); //WarpAgree response packet will make sure everything is dandy
					break;
			}
		}

		//accepting the server's request for a warp.
		//AFAIK this is not ever sent from the client without the client first getting WARP_REQUEST
		private static void WarpAccept(short mapID)
		{
			EOClient client = (EOClient)World.Instance.Client;
			if (!client.ConnectedAndInitialized) return;
			
			Packet builder = new Packet(PacketFamily.Warp, PacketAction.Accept);
			builder.AddShort(mapID);

			client.SendPacket(builder);
		}

		//applying the data post-warp to the character
		public static void WarpAgree(Packet pkt)
		{
			RequestedMap = -1;
			if (pkt.GetChar() != 2) return;

			short mapID = pkt.GetShort();
			WarpAnimation anim = (WarpAnimation) pkt.GetChar();

			int numOtherCharacters = pkt.GetChar();
			if (pkt.GetByte() != 255) return;

			List<EndlessClient.Character> otherCharacters = new List<EndlessClient.Character>(numOtherCharacters - 1);
			for (int i = 0; i < numOtherCharacters; ++i)
			{
				EndlessClient.Character newChar = new EndlessClient.Character(pkt);
				if(newChar.ID == World.Instance.MainPlayer.ActiveCharacter.ID)
					World.Instance.MainPlayer.ActiveCharacter.ApplyData(newChar);
				else
					otherCharacters.Add(newChar);
				if (pkt.GetByte() != 255) return;
			}

			while (pkt.PeekByte() != 255)
			{
				pkt.Skip(6);
				//todo: npcs (see REFRESH_REPLY)
			}

			if (pkt.GetByte() != 255) return;

			while (pkt.ReadPos != pkt.Length)
			{
				World.Instance.ActiveMapRenderer.MapItems.Add(new MapItem
				{
					uid = pkt.GetShort(),
					id = pkt.GetShort(),
					x = pkt.GetByte(),
					y = pkt.GetByte(),
					amount = pkt.GetThree()
				});
			}

		}
	}
}
