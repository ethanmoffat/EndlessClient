// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;

namespace EOLib.Net.API
{
	public struct ChestData
	{
		private readonly byte x, y;
		private readonly List<Tuple<short, int>> items;

		public byte X { get { return x; } }
		public byte Y { get { return y; } }
		public IList<Tuple<short, int>> Items { get { return items.AsReadOnly(); } }

		internal ChestData(Packet pkt, bool containsCoords)
		{
			x = containsCoords ? pkt.GetChar() : byte.MinValue;
			y = containsCoords ? pkt.GetChar() : byte.MinValue;

			int numRemaining = pkt.PeekEndString().Length / 5;
			items = new List<Tuple<short, int>>(numRemaining);
			for (int i = 0; i < numRemaining; ++i)
			{
				items.Add(new Tuple<short, int>(pkt.GetShort(), pkt.GetThree()));
			}
		}
	}

	partial class PacketAPI
	{
		public delegate void ChestDataChangeEvent(short id, int amount, byte weight, byte maxWeight, ChestData data);

		public event Action<ChestData> OnChestOpened;
		public event Action<ChestData> OnChestAgree;
		public event ChestDataChangeEvent OnChestGetItem;
		public event ChestDataChangeEvent OnChestAddItem;

		private void _createChestMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Chest, PacketAction.Open), _handleChestOpen, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Chest, PacketAction.Get), _handleChestGet, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Chest, PacketAction.Agree), _handleChestAgree, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Chest, PacketAction.Reply), _handleChestReply, true);
		}

		public bool ChestOpen(byte x, byte y)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet toSend = new Packet(PacketFamily.Chest, PacketAction.Open);
			toSend.AddChar(x);
			toSend.AddChar(y);

			return m_client.SendPacket(toSend);
		}

		public bool ChestTakeItem(byte x, byte y, short itemID)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Chest, PacketAction.Take);
			pkt.AddChar(x);
			pkt.AddChar(y);
			pkt.AddShort(itemID);

			return m_client.SendPacket(pkt);
		}

		public bool ChestAddItem(byte x, byte y, short itemID, int amount)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Chest, PacketAction.Add);
			pkt.AddChar(x);
			pkt.AddChar(y);
			pkt.AddShort(itemID);
			pkt.AddThree(amount);

			return m_client.SendPacket(pkt);
		}

		/// <summary>
		/// Handler for CHEST_OPEN packet, sent in response to main player opening a chest
		/// </summary>
		private void _handleChestOpen(Packet pkt)
		{
			if (OnChestOpened != null)
				OnChestOpened(new ChestData(pkt, true));
		}

		/// <summary>
		/// Handler for CHEST_GET packet, sent as confirmation to character that item is being taken
		/// </summary>
		private void _handleChestGet(Packet pkt)
		{
			if (OnChestGetItem == null) return;
			short takenID = pkt.GetShort();
			int takenAmount = pkt.GetThree();
			byte characterWeight = pkt.GetChar();
			byte characterMaxWeight = pkt.GetChar();
			ChestData data = new ChestData(pkt, false);
			OnChestGetItem(takenID, takenAmount, characterWeight, characterMaxWeight, data);
		}

		/// <summary>
		/// Handler for CHEST_AGREE packet, sent as update to other characters near a chest that was modified by another player
		/// </summary>
		private void _handleChestAgree(Packet pkt)
		{
			if (OnChestAgree != null)
				OnChestAgree(new ChestData(pkt, false));
		}

		/// <summary>
		/// Handler for CHEST_REPLY packet, sent in response to main player adding an item to a chest
		/// </summary>
		private void _handleChestReply(Packet pkt)
		{
			if (OnChestAddItem == null) return;

			short remainingID = pkt.GetShort();
			int remainingAmount = pkt.GetInt();
			byte characterWeight = pkt.GetChar();
			byte characterMaxWeight = pkt.GetChar();
			ChestData data = new ChestData(pkt, false);
			OnChestAddItem(remainingID, remainingAmount, characterWeight, characterMaxWeight, data);
		}
	}
}
