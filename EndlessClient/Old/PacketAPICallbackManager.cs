using System;
using System.Collections.Generic;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Old;
using EOLib.Domain.Character;
using EOLib.Localization;
using EOLib.Net.API;
using XNAControls.Old;

namespace EndlessClient.Old
{
    public sealed class PacketAPICallbackManager
    {
        private readonly PacketAPI m_packetAPI;
        private readonly EOGame m_game;

        public PacketAPICallbackManager(PacketAPI apiObj, EOGame game)
        {
            m_packetAPI = apiObj;
            m_game = game;
        }

        public void AssignCallbacks()
        {
            //trade
            m_packetAPI.OnTradeRequested += _tradeRequested;
            m_packetAPI.OnTradeOpen += _tradeOpen;
            m_packetAPI.OnTradeCancel += _tradeCancel;
            m_packetAPI.OnTradeOtherPlayerAgree += _tradeRemotePlayerAgree;
            m_packetAPI.OnTradeYouAgree += _tradeSetLocalPlayerAgree;
            m_packetAPI.OnTradeOfferUpdate += _tradeOfferUpdate;
            m_packetAPI.OnTradeCompleted += _tradeCompleted;
        }

        private void _tradeRequested(short playerID, string name)
        {
            if (!OldWorld.Instance.Interaction)
                return;

            EOMessageBox.Show(char.ToUpper(name[0]) + name.Substring(1) + " ", DialogResourceID.TRADE_REQUEST, EODialogButtons.OkCancel,
                    EOMessageBoxStyle.SmallDialogSmallHeader, (o, e) =>
                    {
                        if (e.Result == XNADialogResult.OK && !m_packetAPI.TradeAcceptRequest(playerID))
                            m_game.DoShowLostConnectionDialogAndReturnToMainMenu();
                    });
        }

        private void _tradeOpen(short p1, string p1name, short p2, string p2name)
        {
            TradeDialog dlg = new TradeDialog(m_packetAPI);
            dlg.InitPlayerInfo(p1, p1name, p2, p2name);

            string otherName;
            if (p1 == OldWorld.Instance.MainPlayer.ActiveCharacter.ID)
                otherName = p2name;
            else if (p2 == OldWorld.Instance.MainPlayer.ActiveCharacter.ID)
                otherName = p1name;
            else
                throw new ArgumentException("Invalid player ID for this trade session!", nameof(p1));

            //m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_TRADE_YOU_ARE_TRADING_WITH,
            //        otherName + " " + OldWorld.GetString(EOResourceID.STATUS_LABEL_DRAG_AND_DROP_ITEMS));
        }

        private void _tradeCancel(short otherPlayerID)
        {
            if (TradeDialog.Instance == null) return;
            TradeDialog.Instance.Close(XNADialogResult.NO_BUTTON_PRESSED);
            //m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_TRADE_ABORTED);
        }

        private void _tradeRemotePlayerAgree(short otherPlayerID, bool agree)
        {
            if (TradeDialog.Instance == null) return;
            TradeDialog.Instance.SetPlayerAgree(false, agree);
        }

        private void _tradeSetLocalPlayerAgree(bool agree)
        {
            if (TradeDialog.Instance == null) return;
            TradeDialog.Instance.SetPlayerAgree(true, agree);
        }

        private void _tradeOfferUpdate(short id1, List<InventoryItem> items1, short id2, List<InventoryItem> items2)
        {
            if (TradeDialog.Instance == null) return;
            TradeDialog.Instance.SetPlayerItems(id1, items1);
            TradeDialog.Instance.SetPlayerItems(id2, items2);
        }

        private void _tradeCompleted(short id1, List<InventoryItem> items1, short id2, List<InventoryItem> items2)
        {
            if (TradeDialog.Instance == null) return;
            TradeDialog.Instance.CompleteTrade(id1, items1, id2, items2);
        }
    }
}
