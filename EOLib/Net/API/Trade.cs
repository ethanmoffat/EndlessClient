using System;
using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Net.Handlers;

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

        private void _handleTradeRequest(OldPacket pkt)
        {
            pkt.Skip(1); //something - will always be 123 from this client
            short playerID = pkt.GetShort();
            string name = pkt.GetEndString();

            if (OnTradeRequested != null)
                OnTradeRequested(playerID, name);
        }

        private void _handleTradeOpen(OldPacket pkt)
        {
            if (OnTradeOpen == null) return;

            short player1ID = pkt.GetShort();
            string player1Name = pkt.GetBreakString();
            short player2ID = pkt.GetShort();
            string player2Name = pkt.GetBreakString();

            OnTradeOpen(player1ID, player1Name, player2ID, player2Name);
        }

        private void _handleTradeReply(OldPacket pkt)
        {
            _sharedTradeDataProcess(pkt, OnTradeOfferUpdate);
        }

        //sent in response to you agreeing
        private void _handleTradeSpec(OldPacket pkt)
        {
            if (OnTradeYouAgree != null)
                OnTradeYouAgree(pkt.GetChar() != 0);
        }

        //sent when your trade partner agrees
        private void _handleTradeAgree(OldPacket pkt)
        {
            if (OnTradeOtherPlayerAgree != null)
                OnTradeOtherPlayerAgree(pkt.GetShort(), pkt.GetChar() != 0);
        }

        //both parties agree to the trade - trade completed
        private void _handleTradeUse(OldPacket pkt)
        {
            _sharedTradeDataProcess(pkt, OnTradeCompleted);
        }

        private void _handleTradeClose(OldPacket pkt)
        {
            if (OnTradeCancel != null)
                OnTradeCancel(pkt.GetShort());
        }

        private void _sharedTradeDataProcess(OldPacket pkt, TradeUpdateEvent handler)
        {
            if (handler == null) return;

            short player1ID = pkt.GetShort();
            List<InventoryItem> player1Items = new List<InventoryItem>();
            while (pkt.PeekByte() != 255)
            {
                player1Items.Add(new InventoryItem(pkt.GetShort(), pkt.GetInt()));
            }
            pkt.Skip(1);

            short player2ID = pkt.GetShort();
            List<InventoryItem> player2Items = new List<InventoryItem>();
            while (pkt.PeekByte() != 255)
            {
                player2Items.Add(new InventoryItem(pkt.GetShort(), pkt.GetInt()));
            }
            pkt.Skip(1);

            handler(player1ID, player1Items, player2ID, player2Items);
        }
    }
}
