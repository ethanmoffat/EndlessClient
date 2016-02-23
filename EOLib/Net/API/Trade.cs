// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;

namespace EOLib.Net.API
{
	partial class PacketAPI
	{
		public delegate void TradeRequestEvent(short playerID, string name);
		public delegate void TradeOpenEvent(short player1ID, string player1Name, short player2ID, string player2Name);
		public delegate void TradeUpdateEvent(short id1, List<InventoryItem> items1, short id2, List<InventoryItem> items2);
		public event TradeRequestEvent OnTradeRequested;
		public event TradeOpenEvent OnTradeOpen;
		public event TradeUpdateEvent OnTradeOfferUpdate;
		public event Action<short> OnTradeCancel;
		public event Action<bool> OnTradeYouAgree;
		public event Action<short, bool> OnTradeOtherPlayerAgree;
		public event TradeUpdateEvent OnTradeCompleted;

		private void _createTradeMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Trade, PacketAction.Request), _handleTradeRequest, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Trade, PacketAction.Open), _handleTradeOpen, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Trade, PacketAction.Reply), _handleTradeReply, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Trade, PacketAction.Spec), _handleTradeSpec, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Trade, PacketAction.Agree), _handleTradeAgree, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Trade, PacketAction.Use), _handleTradeUse, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Trade, PacketAction.Close), _handleTradeClose, true);
		}

		/// <summary>
		/// Request a trade with another player
		/// </summary>
		/// <param name="characterID">ID of the other player's character</param>
		public bool TradeRequest(short characterID)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Trade, PacketAction.Request);
			pkt.AddChar(123); //?
			pkt.AddShort(characterID);

			return m_client.SendPacket(pkt);
		}

		/// <summary>
		/// Accept another players request for trade
		/// </summary>
		/// <param name="characterID">ID of the other player's character</param>
		public bool TradeAcceptRequest(short characterID)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Trade, PacketAction.Accept);
			pkt.AddChar(123); //?
			pkt.AddShort(characterID);

			return m_client.SendPacket(pkt);
		}

		/// <summary>
		/// Remove an item from a pending trade offer
		/// </summary>
		/// <param name="itemID">Item ID of the item to remove</param>
		public bool TradeRemoveItem(short itemID)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Trade, PacketAction.Remove);
			pkt.AddShort(itemID);

			return m_client.SendPacket(pkt);
		}

		/// <summary>
		/// Add an item to a pending trade offer
		/// </summary>
		/// <param name="itemID">Item ID of the item to add</param>
		/// <param name="amount">Amount of the item to add</param>
		public bool TradeAddItem(short itemID, int amount)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Trade, PacketAction.Add);
			pkt.AddShort(itemID);
			pkt.AddInt(amount);

			return m_client.SendPacket(pkt);
		}

		/// <summary>
		/// Set the agree flag for a pending trade offer
		/// </summary>
		/// <param name="agree">True to agree, false to un-agree</param>
		public bool TradeAgree(bool agree)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Trade, PacketAction.Agree);
			pkt.AddChar((byte)(agree ? 1 : 0));

			return m_client.SendPacket(pkt);
		}

		/// <summary>
		/// Cancel a pending trade
		/// </summary>
		public bool TradeClose()
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.Trade, PacketAction.Close);
			pkt.AddChar(123); //?

			return m_client.SendPacket(pkt);
		}

		private void _handleTradeRequest(Packet pkt)
		{
			pkt.Skip(1); //something - will always be 123 from this client
			short playerID = pkt.GetShort();
			string name = pkt.GetEndString();

			if (OnTradeRequested != null)
				OnTradeRequested(playerID, name);
		}

		private void _handleTradeOpen(Packet pkt)
		{
			if (OnTradeOpen == null) return;

			short player1ID = pkt.GetShort();
			string player1Name = pkt.GetBreakString();
			short player2ID = pkt.GetShort();
			string player2Name = pkt.GetBreakString();

			OnTradeOpen(player1ID, player1Name, player2ID, player2Name);
		}

		private void _handleTradeReply(Packet pkt)
		{
			_sharedTradeDataProcess(pkt, OnTradeOfferUpdate);
		}

		//sent in response to you agreeing
		private void _handleTradeSpec(Packet pkt)
		{
			if (OnTradeYouAgree != null)
				OnTradeYouAgree(pkt.GetChar() != 0);
		}

		//sent when your trade partner agrees
		private void _handleTradeAgree(Packet pkt)
		{
			if (OnTradeOtherPlayerAgree != null)
				OnTradeOtherPlayerAgree(pkt.GetShort(), pkt.GetChar() != 0);
		}

		//both parties agree to the trade - trade completed
		private void _handleTradeUse(Packet pkt)
		{
			_sharedTradeDataProcess(pkt, OnTradeCompleted);
		}

		private void _handleTradeClose(Packet pkt)
		{
			if (OnTradeCancel != null)
				OnTradeCancel(pkt.GetShort());
		}

		private void _sharedTradeDataProcess(Packet pkt, TradeUpdateEvent handler)
		{
			if (handler == null) return;

			short player1ID = pkt.GetShort();
			List<InventoryItem> player1Items = new List<InventoryItem>();
			while (pkt.PeekByte() != 255)
			{
				player1Items.Add(new InventoryItem { id = pkt.GetShort(), amount = pkt.GetInt() });
			}
			pkt.Skip(1);

			short player2ID = pkt.GetShort();
			List<InventoryItem> player2Items = new List<InventoryItem>();
			while (pkt.PeekByte() != 255)
			{
				player2Items.Add(new InventoryItem { id = pkt.GetShort(), amount = pkt.GetInt() });
			}
			pkt.Skip(1);

			handler(player1ID, player1Items, player2ID, player2Items);
		}
	}
}
