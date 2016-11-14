// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EndlessClient.Audio;
using EndlessClient.Dialogs;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Chat;
using EOLib.Domain.Map;
using EOLib.IO;
using EOLib.IO.Extensions;
using EOLib.Localization;
using EOLib.Net.API;
using XNAControls;
using ChatType = EOLib.Domain.Chat.ChatType;

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
            m_packetAPI.OnPlayerEnterMap += _playerEnterMap;
            m_packetAPI.OnNPCEnterMap += _npcEnterMap;

            m_packetAPI.OnAdminHiddenChange += _adminHiddenChange;
            m_packetAPI.OnOtherPlayerAttack += _otherPlayerAttack;

            m_packetAPI.OnPlayerAvatarRemove += _playerAvatarRemove;
            m_packetAPI.OnPlayerAvatarChange += _playerAvatarChange;

            m_packetAPI.OnPlayerPaperdollChange += _playerPaperdollChange;
            m_packetAPI.OnViewPaperdoll += _playerViewPaperdoll;

            m_packetAPI.OnDoorOpen += _doorOpen;

            //chest related
            m_packetAPI.OnChestOpened += _chestOpen;
            m_packetAPI.OnChestAgree += _chestAgree;
            m_packetAPI.OnChestAddItem += _chestAddItem;
            m_packetAPI.OnChestGetItem += _chestGetItem;

            //recovery related
            m_packetAPI.OnPlayerRecover += _playerRecover;
            m_packetAPI.OnRecoverReply += _recoverReply;
            m_packetAPI.OnStatsList += _statsList;
            m_packetAPI.OnPlayerHeal += _playerHeal;

            //item related
            m_packetAPI.OnGetItemFromMap += _getItemFromMap;
            m_packetAPI.OnRemoveItemFromMap += _removeItemFromMap;
            m_packetAPI.OnJunkItem += _junkItem;
            m_packetAPI.OnDropItem += _dropItem;
            m_packetAPI.OnUseItem += _useItem;
            m_packetAPI.OnItemChange += _itemChange;

            m_packetAPI.OnMapMutation += _mapMutate;

            //npc related
            m_packetAPI.OnNPCWalk += _npcWalk;
            m_packetAPI.OnNPCAttack += _npcAttack;
            m_packetAPI.OnNPCChat += _npcChat;
            m_packetAPI.OnNPCLeaveMap += _npcLeaveView;
            m_packetAPI.OnNPCKilled += _npcKilled;
            m_packetAPI.OnNPCTakeDamage += _npcTakeDamage;
            m_packetAPI.OnPlayerLevelUp += _playerLevelUp;
            m_packetAPI.OnRemoveChildNPCs += _removeChildNPCs;

            //bank related
            m_packetAPI.OnBankOpen += _bankOpen;
            m_packetAPI.OnBankChange += _bankChange;

            //shop
            m_packetAPI.OnShopOpen += _shopOpen;
            m_packetAPI.OnShopTradeItem += _shopTrade;
            m_packetAPI.OnShopCraftItem += _shopCraft;

            //locker
            m_packetAPI.OnLockerOpen += _lockerOpen;
            m_packetAPI.OnLockerItemChange += _lockerItemChange;
            m_packetAPI.OnLockerUpgrade += _lockerUpgrade;

            m_packetAPI.OnOtherPlayerEmote += _playerEmote;

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

            //map effects
            m_packetAPI.OnTimedSpike += _timedSpike;
            m_packetAPI.OnPlayerTakeSpikeDamage += _mainPlayerSpikeDamage;
            m_packetAPI.OnOtherPlayerTakeSpikeDamage += _otherPlayerSpikeDamage;
            m_packetAPI.OnTimedMapDrainHP += _mapDrainHP;
            m_packetAPI.OnTimedMapDrainTP += _mapDrainTP;
            m_packetAPI.OnEffectPotion += _otherPlayerEffectPotion;

            //quests
            m_packetAPI.OnQuestDialog += _questDialog;
            m_packetAPI.OnViewQuestProgress += _questProgress;
            m_packetAPI.OnViewQuestHistory += _questHistory;
            m_packetAPI.OnStatusMessage += _setStatusLabel;

            m_packetAPI.OnPlaySoundEffect += _playSoundEffect;

            //spell casting
            m_packetAPI.OnOtherPlayerStartCastSpell += _playerStartCastSpell;
            m_packetAPI.OnOtherPlayerCastSpellSelf += _otherPlayerCastSpellSelf;
            m_packetAPI.OnCastSpellSelf += _mainPlayerCastSpellSelf;
            m_packetAPI.OnCastSpellTargetOther += _playerCastTargetSpell;
            m_packetAPI.OnCastSpellTargetGroup += _playerCastGroupSpell;
        }

        private void _playerEnterMap(CharacterData data, WarpAnimation anim)
        {
            OldWorld.Instance.ActiveMapRenderer.AddOtherPlayer(data, anim);
        }

        private void _npcEnterMap(NPCData obj)
        {
            if (OldWorld.Instance.ActiveMapRenderer == null) return;
            OldWorld.Instance.ActiveMapRenderer.AddOtherNPC(obj);
        }

        private void _adminHiddenChange(short id, bool hidden)
        {
            if (OldWorld.Instance.MainPlayer.ActiveCharacter.ID == id)
                OldWorld.Instance.MainPlayer.ActiveCharacter.RenderData.SetHidden(hidden);
            else
                OldWorld.Instance.ActiveMapRenderer.OtherPlayerHide(id, hidden);
        }

        private void _otherPlayerAttack(short playerid, EODirection dir)
        {
            OldWorld.Instance.ActiveMapRenderer.OtherPlayerAttack(playerid, dir);
        }

        private void _playerAvatarRemove(short id, WarpAnimation anim)
        {
            OldWorld.Instance.ActiveMapRenderer.RemoveOtherPlayer(id, anim);
        }

        private void _playerAvatarChange(AvatarData _data)
        {
            switch (_data.Slot)
            {
                case AvatarSlot.Clothes:
                    OldWorld.Instance.ActiveMapRenderer.UpdateOtherPlayerRenderData(_data.ID, _data.Sound, new CharRenderData
                    {
                        boots = _data.Boots,
                        armor = _data.Armor,
                        hat = _data.Hat,
                        shield = _data.Shield,
                        weapon = _data.Weapon
                    });
                    break;
                case AvatarSlot.Hair:
                    OldWorld.Instance.ActiveMapRenderer.UpdateOtherPlayerHairData(_data.ID, _data.HairColor, _data.HairStyle);
                    break;
                case AvatarSlot.HairColor:
                    OldWorld.Instance.ActiveMapRenderer.UpdateOtherPlayerHairData(_data.ID, _data.HairColor);
                    break;
            }
        }

        private void _playerPaperdollChange(PaperdollEquipData _data)
        {
            OldCharacter c;
            if (!_data.ItemWasUnequipped)
            {
                var rec = OldWorld.Instance.EIF[_data.ItemID];
                //update inventory
                (c = OldWorld.Instance.MainPlayer.ActiveCharacter).UpdateInventoryItem(_data.ItemID, _data.ItemAmount);
                //equip item
                c.EquipItem(rec.Type, (short) rec.ID, (short) rec.DollGraphic, true, (sbyte) _data.SubLoc);
                //add to paperdoll dialog
                if (EOPaperdollDialog.Instance != null)
                    EOPaperdollDialog.Instance.SetItem(rec.GetEquipLocation() + _data.SubLoc, rec);
            }
            else
            {
                c = OldWorld.Instance.MainPlayer.ActiveCharacter;
                //update inventory
                c.UpdateInventoryItem(_data.ItemID, 1, true); //true: add to existing quantity
                //unequip item
                c.UnequipItem(OldWorld.Instance.EIF[_data.ItemID].Type, _data.SubLoc);
            }
            c.UpdateStatsAfterEquip(_data);
        }

        private void _playerViewPaperdoll(PaperdollDisplayData _data)
        {
            if (EOPaperdollDialog.Instance != null) return;

            OldCharacter c;
            if (OldWorld.Instance.MainPlayer.ActiveCharacter.ID == _data.PlayerID)
            {
                //paperdoll requested for main player, all info should be up to date
                c = OldWorld.Instance.MainPlayer.ActiveCharacter;
                Array.Copy(_data.Paperdoll.ToArray(), c.PaperDoll, (int) EquipLocation.PAPERDOLL_MAX);
            }
            else
            {
                if ((c = OldWorld.Instance.ActiveMapRenderer.GetOtherPlayerByID(_data.PlayerID)) != null)
                {
                    c.Class = _data.Class;
                    c.RenderData.SetGender(_data.Gender);
                    c.Title = _data.Title;
                    c.GuildName = _data.Guild;
                    Array.Copy(_data.Paperdoll.ToArray(), c.PaperDoll, (int) EquipLocation.PAPERDOLL_MAX);
                }
            }

            if (c != null)
            {
                EOPaperdollDialog.Show(m_packetAPI, c, _data);
            }
        }

        private void _doorOpen(byte x, byte y)
        {
            OldWorld.Instance.ActiveMapRenderer.OnDoorOpened(x, y);
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
            OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amount, weight, maxWeight);
            ChestDialog.Instance.InitializeItems(data.Items);
            m_game.Hud.RefreshStats();
        }

        private void _chestGetItem(short id, int amount, byte weight, byte maxWeight, ChestData data)
        {
            OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amount, weight, maxWeight, true);
            ChestDialog.Instance.InitializeItems(data.Items);
            m_game.Hud.RefreshStats();
        }

        private void _playerRecover(short hp, short tp)
        {
            OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.HP = hp;
            OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.TP = tp;
            if (m_game.Hud != null)
                m_game.Hud.RefreshStats();
        }

        private void _recoverReply(int exp, short karma, byte level, short statpoints, short skillpoints)
        {
            OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.Experience = exp;
            OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.Karma = karma;
            if (level > 0)
                OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.Level = level;

            OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.StatPoints = statpoints;
            OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.SkillPoints = skillpoints;

            m_game.Hud.RefreshStats();
        }

        private void _statsList(DisplayStats _data)
        {

            CharStatData localStats = OldWorld.Instance.MainPlayer.ActiveCharacter.Stats;
            if (_data.IsStatsData)
                localStats.StatPoints = _data.StatPoints;
            else
                OldWorld.Instance.MainPlayer.ActiveCharacter.Class = _data.Class;
            localStats.Str = _data.Str;
            localStats.Int = _data.Int;
            localStats.Wis = _data.Wis;
            localStats.Agi = _data.Agi;
            localStats.Con = _data.Con;
            localStats.Cha = _data.Cha;
            localStats.MaxHP = _data.MaxHP;
            localStats.MaxTP = _data.MaxTP;
            localStats.SP = _data.MaxSP;
            localStats.MaxSP = _data.MaxSP;
            OldWorld.Instance.MainPlayer.ActiveCharacter.MaxWeight = _data.MaxWeight;
            localStats.MinDam = _data.MinDam;
            localStats.MaxDam = _data.MaxDam;
            localStats.Accuracy = _data.Accuracy;
            localStats.Evade = _data.Evade;
            localStats.Armor = _data.Armor;
            m_game.Hud.RefreshStats();
        }

        private void _playerHeal(short playerid, int healamount, byte percenthealth)
        {
            OldWorld.Instance.ActiveMapRenderer.OtherPlayerHeal(playerid, healamount, percenthealth);
        }

        private void _getItemFromMap(short uid, short id, int amountTaken, byte weight, byte maxWeight)
        {

            if (uid != 0) //$si command has UniqueID of 0 since we're creating a new item from nothing
            {
                OldWorld.Instance.ActiveMapRenderer.UpdateMapItemAmount(uid, amountTaken);
            }

            OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amountTaken, weight, maxWeight, true);

            var rec = OldWorld.Instance.EIF[id];
            m_game.Hud.AddChat(ChatTab.System, "", string.Format("{0} {1} {2}", OldWorld.GetString(EOResourceID.STATUS_LABEL_ITEM_PICKUP_YOU_PICKED_UP), amountTaken, rec.Name), ChatIcon.UpArrow);
            m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_ITEM_PICKUP_YOU_PICKED_UP, string.Format(" {0} {1}", amountTaken, rec.Name));
        }

        private void _junkItem(short id, int amountRemoved, int amountRemaining, byte weight, byte maxWeight)
        {
            OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amountRemaining, weight, maxWeight);

            var rec = OldWorld.Instance.EIF[id];
            m_game.Hud.AddChat(ChatTab.System, "", string.Format("{0} {1} {2}", OldWorld.GetString(EOResourceID.STATUS_LABEL_ITEM_JUNK_YOU_JUNKED), amountRemoved, rec.Name), ChatIcon.DownArrow);
            m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_ITEM_JUNK_YOU_JUNKED, string.Format(" {0} {1}", amountRemoved, rec.Name));
        }

        private void _dropItem(int characterAmount, byte weight, byte maxWeight, OldMapItem item)
        {
            OldWorld.Instance.ActiveMapRenderer.AddMapItem(item);
            if (characterAmount >= 0) //will be -1 when another player drops
            {
                OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(item.ItemID, characterAmount, weight, maxWeight);

                var rec = OldWorld.Instance.EIF[item.ItemID];
                m_game.Hud.AddChat(ChatTab.System, "",
                        string.Format("{0} {1} {2}", OldWorld.GetString(EOResourceID.STATUS_LABEL_ITEM_DROP_YOU_DROPPED), item.Amount, rec.Name),
                        ChatIcon.DownArrow);
                m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_ITEM_DROP_YOU_DROPPED,
                        string.Format(" {0} {1}", item.Amount, rec.Name));
            }
        }

        private void _useItem(ItemUseData data)
        {
            OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(data.ItemID, data.CharacterAmount, data.Weight, data.MaxWeight);
            switch (data.Type)
            {
                case ItemType.Teleport: /*Warp packet handles the rest!*/ break;
                case ItemType.Heal:
                    {
                        OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.HP = data.HP;
                        OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.TP = data.TP;

                        int percent = (int)Math.Round(100.0 * ((double)data.HP / OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.MaxHP));

                        if (data.HPGain > 0)
                            OldWorld.Instance.ActiveCharacterRenderer.SetDamageCounterValue(data.HPGain, percent, true);
                        m_game.Hud.RefreshStats();
                    }
                    break;
                case ItemType.HairDye:
                    {
                        OldWorld.Instance.MainPlayer.ActiveCharacter.RenderData.SetHairColor(data.HairColor);
                    }
                    break;
                case ItemType.Beer:
                    OldWorld.Instance.ActiveCharacterRenderer.MakeDrunk();
                    m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_ITEM_USE_DRUNK);
                    break;
                case ItemType.EffectPotion: //todo: effect potions for other players
                    OldWorld.Instance.ActiveCharacterRenderer.ShowPotionAnimation(data.EffectID);
                    break;
                case ItemType.CureCurse:
                    {
                        //actually remove the item(s) from the main character
                        OldCharacter c = OldWorld.Instance.MainPlayer.ActiveCharacter;
                        for (int i = 0; i < (int)EquipLocation.PAPERDOLL_MAX; ++i)
                        {
                            int nextID = c.PaperDoll[i];
                            if (nextID > 0 && OldWorld.Instance.EIF[nextID].Special == ItemSpecial.Cursed)
                            {
                                c.PaperDoll[i] = 0;
                                switch ((EquipLocation)i)
                                {
                                    case EquipLocation.Boots: c.RenderData.SetBoots(0); break;
                                    case EquipLocation.Armor: c.RenderData.SetArmor(0); break;
                                    case EquipLocation.Hat: c.RenderData.SetHat(0); break;
                                    case EquipLocation.Shield: c.RenderData.SetShield(0); break;
                                    case EquipLocation.Weapon: c.RenderData.SetWeapon(0); break;
                                }
                            }
                        }

                        //update main character's stats
                        CharStatData s = c.Stats;
                        s.MaxHP = data.CureStats.MaxHP;
                        s.MaxTP = data.CureStats.MaxTP;
                        s.Str = data.CureStats.Str;
                        s.Int = data.CureStats.Int;
                        s.Wis = data.CureStats.Wis;
                        s.Agi = data.CureStats.Agi;
                        s.Con = data.CureStats.Con;
                        s.Cha = data.CureStats.Cha;
                        s.MinDam = data.CureStats.MinDam;
                        s.MaxDam = data.CureStats.MaxDam;
                        s.Accuracy = data.CureStats.Accuracy;
                        s.Evade = data.CureStats.Evade;
                        s.Armor = data.CureStats.Armor;
                        m_game.Hud.RefreshStats();
                    }
                    break;
                case ItemType.EXPReward:
                    {
                        CharStatData s = OldWorld.Instance.MainPlayer.ActiveCharacter.Stats;
                        if (s.Level < data.RewardStats.Level)
                        {
                            //level up!
                            OldWorld.Instance.MainPlayer.ActiveCharacter.Emote(Emote.LevelUp);
                            OldWorld.Instance.ActiveCharacterRenderer.PlayerEmote();
                            s.Level = data.RewardStats.Level;
                        }
                        s.Experience = data.RewardStats.Exp;
                        s.StatPoints = data.RewardStats.StatPoints;
                        s.SkillPoints = data.RewardStats.SkillPoints;
                        s.MaxHP = data.RewardStats.MaxHP;
                        s.MaxTP = data.RewardStats.MaxTP;
                        s.MaxSP = data.RewardStats.MaxSP;
                    }
                    break;
            }
        }

        private void _itemChange(bool wasItemObtained, short id, int amount, byte weight)
        {
            OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amount, weight,
                OldWorld.Instance.MainPlayer.ActiveCharacter.MaxWeight, wasItemObtained);
        }

        private void _removeItemFromMap(short itemuid)
        {
            OldWorld.Instance.ActiveMapRenderer.RemoveMapItem(itemuid);
        }

        private void _mapMutate()
        {
            if (File.Exists("maps\\00000.emf"))
            {
                string fmt = string.Format("maps\\{0,5:D5}.emf", OldWorld.Instance.MainPlayer.ActiveCharacter.CurrentMap);
                if (File.Exists(fmt))
                    File.Delete(fmt);
                File.Move("maps\\00000.emf", fmt);
                OldWorld.Instance.Remap();
            }
            else
                throw new FileNotFoundException("Unable to remap the file, something broke");
        }

        private void _npcWalk(byte index, byte x, byte y, EODirection dir)
        {
            if (OldWorld.Instance.ActiveMapRenderer == null) return;
            OldWorld.Instance.ActiveMapRenderer.NPCWalk(index, x, y, dir);
        }

        private void _npcAttack(byte index, bool dead, EODirection dir, short id, int damage, int health)
        {
            OldWorld.Instance.ActiveMapRenderer.NPCAttack(index, dead, dir, id, damage, health);
        }

        private void _npcChat(byte index, string message)
        {
            if (OldWorld.Instance.ActiveMapRenderer == null) return;

            OldWorld.Instance.ActiveMapRenderer.RenderChatMessage(ChatType.NPC, index, message, ChatIcon.Note);
        }

        private void _npcLeaveView(byte index, int damageToNPC, short playerID, EODirection playerDirection, short playerTP = -1, short spellID = -1)
        {
            if (OldWorld.Instance.ActiveMapRenderer == null) return;

            OldWorld.Instance.ActiveMapRenderer.RemoveOtherNPC(index, damageToNPC, playerID, playerDirection, spellID);

            if (playerTP >= 0)
            {
                OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.TP = playerTP;
                m_game.Hud.RefreshStats();
            }
        }

        private void _npcKilled(int newExp)
        {
            int expDif = newExp - OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.Experience;
            OldWorld.Instance.MainPlayer.ActiveCharacter.GainExp(expDif);

            m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_YOU_GAINED_EXP, string.Format(" {0} EXP", expDif));
            m_game.Hud.AddChat(ChatTab.System, "", string.Format("{0} {1} EXP", OldWorld.GetString(EOResourceID.STATUS_LABEL_YOU_GAINED_EXP), expDif), ChatIcon.Star);
        }

        private void _npcTakeDamage(byte npcIndex, short fromPlayerID, EODirection fromDirection, int damageToNPC, int npcPctHealth, short spellID, short fromTP)
        {
            OldWorld.Instance.ActiveMapRenderer.NPCTakeDamage(npcIndex, fromPlayerID, fromDirection, damageToNPC, npcPctHealth, spellID);

            if (fromTP >= 0)
            {
                OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.TP = fromTP;
                m_game.Hud.RefreshStats();
            }
        }

        private void _playerLevelUp(LevelUpStats _stats)
        {
            OldWorld.Instance.MainPlayer.ActiveCharacter.Emote(Emote.LevelUp);
            OldWorld.Instance.ActiveCharacterRenderer.PlayerEmote();

            CharStatData stats = OldWorld.Instance.MainPlayer.ActiveCharacter.Stats;
            stats.Level = _stats.Level;
            stats.StatPoints = _stats.StatPoints;
            stats.SkillPoints = _stats.SkillPoints;
            stats.MaxHP = _stats.MaxHP;
            stats.MaxTP = _stats.MaxTP;
            stats.MaxSP = _stats.MaxSP;
            m_game.Hud.RefreshStats();
        }

        private void _removeChildNPCs(short childNPCID)
        {
            OldWorld.Instance.ActiveMapRenderer.RemoveNPCsWhere(x => x.NPC.Data.ID == childNPCID);
        }

        private void _bankOpen(int gold, int upgrades)
        {
            if (BankAccountDialog.Instance == null) return;
            BankAccountDialog.Instance.AccountBalance = string.Format("{0}", gold);
            BankAccountDialog.Instance.LockerUpgrades = upgrades;
        }

        private void _bankChange(int gold, int bankGold)
        {
            if (BankAccountDialog.Instance == null) return;

            OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(1, gold);
            BankAccountDialog.Instance.AccountBalance = string.Format("{0}", bankGold);
        }

        private void _shopOpen(int shopid, string name, List<ShopItem> tradeitems, List<CraftItem> craftitems)
        {
            if (ShopDialog.Instance == null) return;
            ShopDialog.Instance.SetShopData(shopid, name, tradeitems, craftitems);
        }

        private void _shopTrade(int gold, short itemID, int amount, byte weight, byte maxWeight, bool isBuy)
        {
            OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(1, gold);
            OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(itemID, amount, weight, maxWeight, isBuy);
        }

        private void _shopCraft(short id, byte weight, byte maxWeight, List<InventoryItem> ingredients)
        {
            OldCharacter c = OldWorld.Instance.MainPlayer.ActiveCharacter;
            c.UpdateInventoryItem(id, 1, weight, maxWeight, true);
            foreach (var ingred in ingredients)
                c.UpdateInventoryItem(ingred.ItemID, ingred.Amount);
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
            OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amount, weight, maxWeight, existingAmount);
            LockerDialog.Instance.SetLockerData(items);
        }

        private void _lockerUpgrade(int remaining, byte upgrades)
        {
            if (BankAccountDialog.Instance == null) return;
            OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(1, remaining);
            BankAccountDialog.Instance.LockerUpgrades = upgrades;
            m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_LOCKER_SPACE_INCREASED);
        }

        private void _playerEmote(short playerID, Emote emote)
        {
            if (playerID != OldWorld.Instance.MainPlayer.ActiveCharacter.ID)
                OldWorld.Instance.ActiveMapRenderer.OtherPlayerEmote(playerID, emote);
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
                   XNADialogButtons.OkCancel, EOMessageBoxStyle.SmallDialogSmallHeader,
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

            EOMessageBox.Show(char.ToUpper(name[0]) + name.Substring(1) + " ", DialogResourceID.TRADE_REQUEST, XNADialogButtons.OkCancel,
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
                throw new ArgumentException("Invalid player ID for this trade session!", "p1");

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
                    EOMessageBox.Show(DialogResourceID.SKILL_LEARN_WRONG_CLASS, " " + OldWorld.Instance.ECF.Data[id].Name + "!", XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                    break;
                case SkillMasterReply.ErrorRemoveItems:
                    EOMessageBox.Show(DialogResourceID.SKILL_RESET_CHARACTER_CLEAR_PAPERDOLL, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
                    break;
            }
        }

        private void _statskillLearnSpellSuccess(short id, int remaining)
        {
            OldWorld.Instance.MainPlayer.ActiveCharacter.Spells.Add(new InventorySpell(id, 0));
            if (SkillmasterDialog.Instance != null)
                SkillmasterDialog.Instance.RemoveSkillByIDFromLearnList(id);
            OldWorld.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(1, remaining);
            m_game.Hud.AddNewSpellToActiveSpellsByID(id);
        }

        private void _statskillForgetSpell(short id)
        {
            OldWorld.Instance.MainPlayer.ActiveCharacter.Spells.RemoveAll(_spell => _spell.ID == id);
            EOMessageBox.Show(DialogResourceID.SKILL_FORGET_SUCCESS, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);

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
            EOMessageBox.Show(DialogResourceID.SKILL_RESET_CHARACTER_COMPLETE, XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
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

        private void _timedSpike()
        {
            OldWorld.Instance.ActiveMapRenderer.PlayTimedSpikeSoundEffect();
        }

        private void _mainPlayerSpikeDamage(short damage, short hp, short maxhp)
        {
            OldWorld.Instance.ActiveMapRenderer.SpikeDamage(damage, hp, maxhp);
        }

        private void _otherPlayerSpikeDamage(short playerid, byte playerPercentHealth, bool isPlayerDead, int damageAmount)
        {
            OldWorld.Instance.ActiveMapRenderer.SpikeDamage(playerid, playerPercentHealth, isPlayerDead, damageAmount);
        }

        private void _mapDrainHP(short damage, short hp, short maxhp, List<TimedMapHPDrainData> othercharacterdata)
        {
            OldWorld.Instance.ActiveMapRenderer.DrainHPFromPlayers(damage, hp, maxhp, othercharacterdata);
        }

        private void _mapDrainTP(short amount, short tp, short maxtp)
        {
            OldWorld.Instance.ActiveMapRenderer.DrainTPFromMainPlayer(amount, tp, maxtp);
        }

        private void _otherPlayerEffectPotion(short playerID, int effectID)
        {
            OldWorld.Instance.ActiveMapRenderer.ShowPotionEffect(playerID, effectID);
        }

        private void _questDialog(QuestState stateinfo, Dictionary<short, string> dialognames, List<string> pages, Dictionary<short, string> links)
        {
            if (QuestDialog.Instance == null)
                QuestDialog.SetupInstance(m_packetAPI);

            if (QuestDialog.Instance == null)
                throw new InvalidOperationException("Something went wrong creating the instance");

            QuestDialog.Instance.SetDisplayData(stateinfo, dialognames, pages, links);
        }

        private void _questProgress(short numquests, List<InProgressQuestData> questinfo)
        {
            if (QuestProgressDialog.Instance == null) return;

            QuestProgressDialog.Instance.SetInProgressDisplayData(numquests, questinfo);
        }

        private void _questHistory(short numquests, List<string> completedquestnames)
        {
            if (QuestProgressDialog.Instance == null) return;

            QuestProgressDialog.Instance.SetHistoryDisplayData(numquests, completedquestnames);
        }

        private void _setStatusLabel(string message)
        {
            m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, message);
            m_game.Hud.AddChat(ChatTab.System, "", message, ChatIcon.QuestMessage, ChatColor.Server);
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

        private void _playerStartCastSpell(short fromplayerid, short spellid)
        {
            OldWorld.Instance.ActiveMapRenderer.OtherPlayerShoutSpell(fromplayerid, spellid);
        }

        private void _otherPlayerCastSpellSelf(short fromplayerid, short spellid, int spellhp, byte percenthealth)
        {
            OldWorld.Instance.ActiveMapRenderer.PlayerCastSpellSelf(fromplayerid, spellid, spellhp, percenthealth);
        }

        private void _mainPlayerCastSpellSelf(short fromplayerid, short spellid, int spellhp, byte percenthealth, short hp, short tp)
        {
            OldWorld.Instance.ActiveMapRenderer.PlayerCastSpellSelf(fromplayerid, spellid, spellhp, percenthealth);
            OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.HP = hp;
            OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.TP = tp;
            m_game.Hud.RefreshStats();
        }

        private void _playerCastTargetSpell(short targetPlayerID, short fromPlayerID, EODirection sourcePlayerDirection, short spellID, int recoveredHP, byte targetPercentHealth, short targetPlayerCurrentHP)
        {
            OldWorld.Instance.ActiveMapRenderer.PlayerCastSpellTarget(fromPlayerID, targetPlayerID, sourcePlayerDirection, spellID, recoveredHP, targetPercentHealth);

            if (targetPlayerCurrentHP > 0)
            {
                OldWorld.Instance.MainPlayer.ActiveCharacter.Stats.HP = targetPlayerCurrentHP;
                m_game.Hud.RefreshStats();
            }
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
