// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib.IO;

namespace EOLib.Net
{
	internal enum WarpReply
	{
		WarpSameMap = 1,
		WarpNewMap = 2
	}

	public enum WarpAnimation
	{
		None,
		Scroll,
		Admin,
		Invalid = 255
	}

	partial class PacketAPI
	{
		/// <summary>
		/// Defines a delegate for a warp request event (when WARP_REQUEST is received from server)
		/// </summary>
		/// <param name="MapID">Map ID of the requested map</param>
		/// <param name="MapRID">RID (revision ID) of the requested map (on server)</param>
		/// <param name="MapFileSize">File size of the requested map (on server)</param>
		/// <returns>true when the local map matches the server map, false otherwise</returns>
		public delegate bool WarpRequestEvent(short MapID, byte[] MapRID, int MapFileSize);

		/// <summary>
		/// Defines a delegate for a warp agree event (when WARP_AGREE is received from server)
		/// </summary>
		public delegate void WarpAgreeEvent(short MapID, WarpAnimation anim, List<CharacterData> charData, List<NPCData> npcData, List<MapItem> itemData);

		/// <summary>
		/// Occurs when a warp action is sent from the server that requests a new map
		/// </summary>
		public event WarpRequestEvent OnWarpRequestNewMap;

		public event WarpAgreeEvent OnWarpAgree;

		private void _createWarpMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Warp, PacketAction.Request), _handleWarpRequest, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Warp, PacketAction.Agree), _handleWarpAgree, true);
		}

		private void _handleWarpRequest(Packet pkt)
		{
			WarpReply warpType = (WarpReply)pkt.GetChar();
			switch (warpType)
			{
				case WarpReply.WarpSameMap: _warpAccept(pkt.GetShort()); break; //pkt.GetChar() x2 for x,y coords
				case WarpReply.WarpNewMap:
					if (OnWarpRequestNewMap == null)
						return;

					short mapID = pkt.GetShort();
					byte[] mapRid = pkt.GetBytes(4);
					int fileSize = pkt.GetThree();

					if (!OnWarpRequestNewMap(mapID, mapRid, fileSize)) //file check failed if return value is false
					{
						//does WARP_TAKE (which downloads a new map) if we need it
						if (!RequestWarpMap(mapID))
							return;
					}
					_warpAccept(mapID); //WarpAgree response packet will make sure everything is dandy
					break;
			}
		}

		private void _warpAccept(short mapID)
		{
			if (!m_client.ConnectedAndInitialized) return;

			Packet builder = new Packet(PacketFamily.Warp, PacketAction.Accept);
			builder.AddShort(mapID);

			m_client.SendPacket(builder);
		}

		private void _handleWarpAgree(Packet pkt)
		{
			if (pkt.GetChar() != 2 || OnWarpAgree == null) return;

			short mapID = pkt.GetShort();
			WarpAnimation anim = (WarpAnimation)pkt.GetChar();

			int numOtherCharacters = pkt.GetChar();
			if (pkt.GetByte() != 255) return;

			List<CharacterData> otherCharacters = new List<CharacterData>(numOtherCharacters - 1);
			for (int i = 0; i < numOtherCharacters; ++i)
			{
				CharacterData newChar = new CharacterData(pkt);
				otherCharacters.Add(newChar);
				if (pkt.GetByte() != 255) return;
			}

			List<NPCData> otherNPCs = new List<NPCData>();
			while (pkt.PeekByte() != 255)
			{
				otherNPCs.Add(new NPCData(pkt));
			}
			if (pkt.GetByte() != 255) return;

			List<MapItem> otherItems = new List<MapItem>();
			while (pkt.ReadPos < pkt.Length)
			{
				otherItems.Add(new MapItem
				{
					uid = pkt.GetShort(),
					id = pkt.GetShort(),
					x = pkt.GetChar(),
					y = pkt.GetChar(),
					amount = pkt.GetThree(),
					//turn off drop protection for items coming into view - server will validate
					time = DateTime.Now.AddSeconds(-5),
					npcDrop = false,
					playerID = -1
				});
			}

			OnWarpAgree(mapID, anim, otherCharacters, otherNPCs, otherItems);
		}
	}
}
