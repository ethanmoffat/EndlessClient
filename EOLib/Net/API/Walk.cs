// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using EOLib.Domain.Map;
using EOLib.IO.Map;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
	partial class PacketAPI
	{
		public delegate void AddMapItemsEvent(List<MapItem> items);
		public delegate void OtherPlayerWalkEvent(short id, EODirection dir, byte x, byte y);

		public event AddMapItemsEvent OnMainPlayerWalk;
		public event OtherPlayerWalkEvent OnOtherPlayerWalk;

		private void _createWalkMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Walk, PacketAction.Reply), _handleMainPlayerWalk, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Walk, PacketAction.Player), _handleOtherPlayerWalk, true);
		}

		public bool PlayerWalk(EODirection dir, byte destX, byte destY, bool admin = false)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			OldPacket builder = new OldPacket(PacketFamily.Walk, admin ? PacketAction.Admin : PacketAction.Player);
				//change family/action
			builder.AddChar((byte) dir);
			builder.AddThree(DateTime.Now.ToEOTimeStamp());
			builder.AddChar(destX);
			builder.AddChar(destY);

			return m_client.SendPacket(builder);
		}

		private void _handleMainPlayerWalk(OldPacket pkt)
		{
			if (pkt.GetByte() != 255 || pkt.GetByte() != 255 || OnMainPlayerWalk == null)
				return;

			//response contains the map items that are now in range
			int numberOfMapItems = pkt.PeekEndString().Length / 9;
			List<MapItem> items = new List<MapItem>(numberOfMapItems);
			for (int i = 0; i < numberOfMapItems; ++i)
			{
				items.Add(new MapItem
				{
					UniqueID = pkt.GetShort(),
					ItemID = pkt.GetShort(),
					X = pkt.GetChar(),
					Y = pkt.GetChar(),
					Amount = pkt.GetThree()
				});
			}

			OnMainPlayerWalk(items);
		}

		private void _handleOtherPlayerWalk(OldPacket pkt)
		{
			if (OnOtherPlayerWalk == null) return;

			short playerID = pkt.GetShort();
			EODirection dir = (EODirection) pkt.GetChar();
			byte x = pkt.GetChar();
			byte y = pkt.GetChar();

			OnOtherPlayerWalk(playerID, dir, x, y);
		}
	}
}
