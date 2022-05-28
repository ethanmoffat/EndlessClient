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
            //npc related
            m_packetAPI.OnRemoveChildNPCs += _removeChildNPCs;

            //party
            m_packetAPI.OnPartyClose += _partyClose;
            m_packetAPI.OnPartyDataRefresh += _partyDataRefresh;
            m_packetAPI.OnPartyRequest += _partyRequest;
            m_packetAPI.OnPartyMemberJoin += _partyMemberJoin;
            m_packetAPI.OnPartyMemberLeave += _partyMemberLeave;

            //trade
            m_packetAPI.OnTradeRequested += _tradeRequested;
            m_packetAPI.OnTradeOpen += _tradeOpen;
            m_packetAPI.OnTradeCancel += _tradeCancel;
            m_packetAPI.OnTradeOtherPlayerAgree += _tradeRemotePlayerAgree;
            m_packetAPI.OnTradeYouAgree += _tradeSetLocalPlayerAgree;
            m_packetAPI.OnTradeOfferUpdate += _tradeOfferUpdate;
            m_packetAPI.OnTradeCompleted += _tradeCompleted;

            //spell casting
            m_packetAPI.OnCastSpellTargetGroup += _playerCastGroupSpell;
        }

        private void _removeChildNPCs(short childNPCID)
        {
            OldWorld.Instance.ActiveMapRenderer.RemoveNPCsWhere(x => x.NPC.Data.ID == childNPCID);
        }

        private void _partyClose()
        {
            m_game.Hud.CloseParty();
        }

        private void _partyDataRefresh(List<PartyMember> list)
        {
            m_game.Hud.SetPartyData(list);
        }

        private void _partyRequest(PartyRequestType type, short id, string name)
        {
            if (!OldWorld.Instance.Interaction)
                return;

            EOMessageBox.Show(name + " ",
                   type == PartyRequestType.Join ? DialogResourceID.PARTY_GROUP_REQUEST_TO_JOIN : DialogResourceID.PARTY_GROUP_SEND_INVITATION,
                   EODialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader,
                   (o, e) =>
                   {
                       if (e.Result == XNADialogResult.OK)
                       {
                           if (!m_packetAPI.PartyAcceptRequest(type, id))
                               m_game.DoShowLostConnectionDialogAndReturnToMainMenu();
                       }
                   });
        }

        private void _partyMemberJoin(PartyMember member)
        {
            m_game.Hud.AddPartyMember(member);
        }

        private void _partyMemberLeave(short id)
        {
            m_game.Hud.RemovePartyMember(id);
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

            m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_TRADE_YOU_ARE_TRADING_WITH,
                    otherName + " " + OldWorld.GetString(EOResourceID.STATUS_LABEL_DRAG_AND_DROP_ITEMS));
        }

        private void _tradeCancel(short otherPlayerID)
        {
            if (TradeDialog.Instance == null) return;
            TradeDialog.Instance.Close(XNADialogResult.NO_BUTTON_PRESSED);
            m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION, EOResourceID.STATUS_LABEL_TRADE_ABORTED);
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

        private void _playerCastGroupSpell(short spellID, short fromPlayerID, short fromPlayerTP, short spellHPgain, List<GroupSpellTarget> spellTargets)
        {
            OldWorld.Instance.ActiveMapRenderer.PlayerCastSpellGroup(fromPlayerID, spellID, spellHPgain, spellTargets);

            if (fromPlayerID == OldWorld.Instance.MainPlayer.ActiveCharacter.ID)
            {
                OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.TP = fromPlayerTP;
                //m_game.Hud.RefreshStats();
            }
        }
    }
}
