using System;
using System.Collections.Generic;
using System.IO;
using EndlessClient.Audio;
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
            //chest related
            m_packetAPI.OnChestOpened += _chestOpen;
            m_packetAPI.OnChestAgree += _chestAgree;
            m_packetAPI.OnChestAddItem += _chestAddItem;
            m_packetAPI.OnChestGetItem += _chestGetItem;

            m_packetAPI.OnMapMutation += _mapMutate;

            //npc related
            m_packetAPI.OnRemoveChildNPCs += _removeChildNPCs;

            //bank related
            m_packetAPI.OnBankOpen += _bankOpen;
            m_packetAPI.OnBankChange += _bankChange;

            //locker
            m_packetAPI.OnLockerOpen += _lockerOpen;
            m_packetAPI.OnLockerItemChange += _lockerItemChange;
            m_packetAPI.OnLockerUpgrade += _lockerUpgrade;

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

            //skills
            m_packetAPI.OnSkillmasterOpen += _skillmasterOpen;
            m_packetAPI.OnSpellLearnError += _statskillLearnError;
            m_packetAPI.OnSpellLearnSuccess += _statskillLearnSpellSuccess;
            m_packetAPI.OnSpellForget += _statskillForgetSpell;
            m_packetAPI.OnSpellTrain += _statskillTrainSpell;
            m_packetAPI.OnCharacterStatsReset += _statskillReset;

            m_packetAPI.OnPlaySoundEffect += _playSoundEffect;

            //spell casting
            m_packetAPI.OnCastSpellTargetGroup += _playerCastGroupSpell;
        }

        private void _chestOpen(ChestData data)
        {
            if (ChestDialog.Instance == null || data.X != ChestDialog.Instance.CurrentChestX ||
                data.Y != ChestDialog.Instance.CurrentChestY)
                return;

            ChestDialog.Instance.InitializeItems(data.Items);
        }

        private void _chestAgree(ChestData data)
        {
            if (ChestDialog.Instance != null)
                ChestDialog.Instance.InitializeItems(data.Items);
        }

        private void _chestAddItem(short id, int amount, byte weight, byte maxWeight, ChestData data)
        {
            //OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amount, weight, maxWeight);
            ChestDialog.Instance.InitializeItems(data.Items);
            m_game.Hud.RefreshStats();
        }

        private void _chestGetItem(short id, int amount, byte weight, byte maxWeight, ChestData data)
        {
            //OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amount, weight, maxWeight, true);
            ChestDialog.Instance.InitializeItems(data.Items);
            m_game.Hud.RefreshStats();
        }

        private void _mapMutate()
        {
            if (File.Exists("maps\\00000.emf"))
            {
                string fmt = $"maps\\{OldWorld.Instance.MainPlayer.ActiveCharacter.CurrentMap,5:D5}.emf";
                if (File.Exists(fmt))
                    File.Delete(fmt);
                File.Move("maps\\00000.emf", fmt);
                OldWorld.Instance.Remap();
            }
            else
                throw new FileNotFoundException("Unable to remap the file, something broke");
        }

        private void _removeChildNPCs(short childNPCID)
        {
            OldWorld.Instance.ActiveMapRenderer.RemoveNPCsWhere(x => x.NPC.Data.ID == childNPCID);
        }

        private void _bankOpen(int gold, int upgrades)
        {
            if (BankAccountDialog.Instance == null) return;
            BankAccountDialog.Instance.AccountBalance = $"{gold}";
            BankAccountDialog.Instance.LockerUpgrades = upgrades;
        }

        private void _bankChange(int gold, int bankGold)
        {
            if (BankAccountDialog.Instance == null) return;

            //OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(1, gold);
            BankAccountDialog.Instance.AccountBalance = $"{bankGold}";
        }

        private void _lockerOpen(byte x, byte y, List<InventoryItem> items)
        {
            if (LockerDialog.Instance == null || LockerDialog.Instance.X != x || LockerDialog.Instance.Y != y)
                return;
            LockerDialog.Instance.SetLockerData(items);
        }

        private void _lockerItemChange(short id, int amount, byte weight, byte maxWeight, bool existingAmount, List<InventoryItem> items)
        {
            if (LockerDialog.Instance == null) return;
            //OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amount, weight, maxWeight, existingAmount);
            LockerDialog.Instance.SetLockerData(items);
        }

        private void _lockerUpgrade(int remaining, byte upgrades)
        {
            if (BankAccountDialog.Instance == null) return;
            //OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(1, remaining);
            BankAccountDialog.Instance.LockerUpgrades = upgrades;
            m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_LOCKER_SPACE_INCREASED);
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

        private void _skillmasterOpen(SkillmasterData data)
        {
            if (SkillmasterDialog.Instance != null)
                SkillmasterDialog.Instance.SetSkillmasterData(data);
        }

        private void _statskillLearnError(SkillMasterReply reply, short id)
        {
            switch (reply)
            {
                //not sure if this will ever actually be sent because client validates data before trying to learn a skill
                case SkillMasterReply.ErrorWrongClass:
                    EOMessageBox.Show(DialogResourceID.SKILL_LEARN_WRONG_CLASS, " " + OldWorld.Instance.ECF[id].Name + "!", EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                    break;
                case SkillMasterReply.ErrorRemoveItems:
                    EOMessageBox.Show(DialogResourceID.SKILL_RESET_CHARACTER_CLEAR_PAPERDOLL, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                    break;
            }
        }

        private void _statskillLearnSpellSuccess(short id, int remaining)
        {
            OldWorld.Instance.MainPlayer.ActiveCharacter.Spells.Add(new InventorySpell(id, 0));
            if (SkillmasterDialog.Instance != null)
                SkillmasterDialog.Instance.RemoveSkillByIDFromLearnList(id);
            //OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(1, remaining);
            m_game.Hud.AddNewSpellToActiveSpellsByID(id);
        }

        private void _statskillForgetSpell(short id)
        {
            OldWorld.Instance.MainPlayer.ActiveCharacter.Spells.RemoveAll(_spell => _spell.ID == id);
            EOMessageBox.Show(DialogResourceID.SKILL_FORGET_SUCCESS, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);

            m_game.Hud.RemoveSpellFromActiveSpellsByID(id);
        }

        private void _statskillTrainSpell(short skillPtsRemaining, short spellID, short spellLevel)
        {
            var character = OldWorld.Instance.MainPlayer.ActiveCharacter;
            character.Stats.SkillPoints = skillPtsRemaining;

            var spellNdx = character.Spells.FindIndex(x => x.ID == spellID);
            character.Spells[spellNdx] = new InventorySpell(spellID, spellLevel);

            m_game.Hud.RefreshStats();
            m_game.Hud.UpdateActiveSpellLevelByID(spellID, spellLevel);
        }

        private void _statskillReset(StatResetData data)
        {
            OldCharacter c;
            (c = OldWorld.Instance.MainPlayer.ActiveCharacter).Spells.Clear();
            EOMessageBox.Show(DialogResourceID.SKILL_RESET_CHARACTER_COMPLETE, EODialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
            c.Stats.StatPoints = data.StatPoints;
            c.Stats.SkillPoints = data.SkillPoints;
            c.Stats.HP = data.HP;
            c.Stats.MaxHP = data.MaxHP;
            c.Stats.TP = data.TP;
            c.Stats.MaxTP = data.MaxTP;
            c.Stats.SP = data.MaxSP;
            c.Stats.MaxSP = data.MaxSP;
            c.Stats.Str = data.Str;
            c.Stats.Int = data.Int;
            c.Stats.Wis = data.Wis;
            c.Stats.Agi = data.Agi;
            c.Stats.Con = data.Con;
            c.Stats.Cha = data.Cha;
            c.Stats.MinDam = data.MinDam;
            c.Stats.MaxDam = data.MaxDam;
            c.Stats.Accuracy = data.Accuracy;
            c.Stats.Evade = data.Evade;
            c.Stats.Armor = data.Armor;
            m_game.Hud.RefreshStats();
            m_game.Hud.RemoveAllSpells();
        }

        private void _playSoundEffect(int effectID)
        {
            try
            {
                if (OldWorld.Instance.SoundEnabled)
                    m_game.SoundManager.GetSoundEffectRef((SoundEffectID) effectID).Play();
            }
            catch { /* Ignore errors when the sound effect ID from the server is invalid */ }
        }

        private void _playerCastGroupSpell(short spellID, short fromPlayerID, short fromPlayerTP, short spellHPgain, List<GroupSpellTarget> spellTargets)
        {
            OldWorld.Instance.ActiveMapRenderer.PlayerCastSpellGroup(fromPlayerID, spellID, spellHPgain, spellTargets);

            if (fromPlayerID == OldWorld.Instance.MainPlayer.ActiveCharacter.ID)
            {
                OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.TP = fromPlayerTP;
                m_game.Hud.RefreshStats();
            }
        }
    }
}
