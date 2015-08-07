using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EOLib;
using EOLib.Data;
using EOLib.Net;
using XNAControls;

namespace EndlessClient
{
	public sealed class PacketAPICallbackManager
	{
		private static bool s_hasBeenSetup;

		private readonly PacketAPI m_packetAPI;
		private readonly EOGame m_game;

		public PacketAPICallbackManager(PacketAPI apiObj, EOGame game)
		{
			m_packetAPI = apiObj;
			m_game = game;
		}

		public void AssignCallbacks()
		{
			//this should only be done once, otherwise each event will get set again!
			if (s_hasBeenSetup) return;
			s_hasBeenSetup = true;

			m_packetAPI.OnWarpRequestNewMap += World.Instance.CheckMap;
			m_packetAPI.OnWarpAgree += World.Instance.WarpAgreeAction;

			m_packetAPI.OnPlayerEnterMap += _playerEnterMap;
			m_packetAPI.OnNPCEnterMap += _npcEnterMap;

			m_packetAPI.OnMainPlayerWalk += _mainPlayerWalk;
			m_packetAPI.OnOtherPlayerWalk += _otherPlayerWalk;

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

			m_packetAPI.OnServerPingReply += _showPingTime;
			m_packetAPI.OnPlayerFindCommandReply += _showFindCommandResult;

			m_packetAPI.OnPlayerFace += _playerFace;

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

			m_packetAPI.OnMapMutation += _mapMutate;

			//npc related
			m_packetAPI.OnNPCWalk += _npcWalk;
			m_packetAPI.OnNPCAttack += _npcAttack;
			m_packetAPI.OnNPCChat += _npcChat;
			m_packetAPI.OnNPCLeaveMap += _npcLeaveView;
			m_packetAPI.OnNPCKilled += _npcKilled;
			m_packetAPI.OnNPCTakeDamage += _npcTakeDamage;
			m_packetAPI.OnPlayerLevelUp += _playerLevelUp;

			//chat related
			m_packetAPI.OnPlayerChatByID += _chatByPlayerID;
			m_packetAPI.OnPlayerChatByName += _chatByPlayerName;
			m_packetAPI.OnPMRecipientNotFound += _pmRecipientNotFound;
			m_packetAPI.OnMuted += _playerMuted;

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
			//todo: spelltrain event
			m_packetAPI.OnCharacterStatsReset += _statskillReset;
		}

		private void _playerEnterMap(CharacterData data, WarpAnimation anim)
		{
			World.Instance.ActiveMapRenderer.AddOtherPlayer(data, anim);
		}

		private void _npcEnterMap(NPCData obj)
		{
			World.Instance.ActiveMapRenderer.AddOtherNPC(obj);
		}

		private void _mainPlayerWalk(List<MapItem> _list)
		{
			foreach (var item in _list) World.Instance.ActiveMapRenderer.AddMapItem(item);
		}

		private void _otherPlayerWalk(short id, EODirection dir, byte x, byte y)
		{
			World.Instance.ActiveMapRenderer.OtherPlayerWalk(id, dir, x, y);
		}

		private void _adminHiddenChange(short id, bool hidden)
		{
			if (World.Instance.MainPlayer.ActiveCharacter.ID == id)
				World.Instance.MainPlayer.ActiveCharacter.RenderData.SetHidden(hidden);
			else
				World.Instance.ActiveMapRenderer.OtherPlayerHide(id, hidden);
		}

		private void _otherPlayerAttack(short playerid, EODirection dir)
		{
			World.Instance.ActiveMapRenderer.OtherPlayerAttack(playerid, dir);
		}

		private void _playerAvatarRemove(short id, WarpAnimation anim)
		{
			World.Instance.ActiveMapRenderer.RemoveOtherPlayer(id, anim);
		}

		private void _playerAvatarChange(AvatarData _data)
		{
			switch (_data.Slot)
			{
				case AvatarSlot.Clothes:
					World.Instance.ActiveMapRenderer.UpdateOtherPlayerRenderData(_data.ID, _data.Sound, new CharRenderData
					{
						boots = _data.Boots,
						armor = _data.Armor,
						hat = _data.Hat,
						shield = _data.Shield,
						weapon = _data.Weapon
					});
					break;
				case AvatarSlot.Hair:
					World.Instance.ActiveMapRenderer.UpdateOtherPlayerHairData(_data.ID, _data.HairColor, _data.HairStyle);
					break;
				case AvatarSlot.HairColor:
					World.Instance.ActiveMapRenderer.UpdateOtherPlayerHairData(_data.ID, _data.HairColor);
					break;
			}
		}

		private void _playerPaperdollChange(PaperdollEquipData _data)
		{
			Character c;
			if (!_data.ItemWasUnequipped)
			{
				ItemRecord rec = World.Instance.EIF.GetItemRecordByID(_data.ItemID);
				//update inventory
				(c = World.Instance.MainPlayer.ActiveCharacter).UpdateInventoryItem(_data.ItemID, _data.ItemAmount);
				//equip item
				c.EquipItem(rec.Type, (short) rec.ID, (short) rec.DollGraphic, true, (sbyte) _data.SubLoc);
				//add to paperdoll dialog
				if (EOPaperdollDialog.Instance != null)
					EOPaperdollDialog.Instance.SetItem(rec.GetEquipLocation() + _data.SubLoc, rec);
			}
			else
			{
				c = World.Instance.MainPlayer.ActiveCharacter;
				//update inventory
				c.UpdateInventoryItem(_data.ItemID, 1, true); //true: add to existing quantity
				//unequip item
				c.UnequipItem(World.Instance.EIF.GetItemRecordByID(_data.ItemID).Type, _data.SubLoc);
			}
			c.UpdateStatsAfterEquip(_data);
		}

		private void _playerViewPaperdoll(PaperdollDisplayData _data)
		{
			if (EOPaperdollDialog.Instance != null) return;

			Character c;
			if (World.Instance.MainPlayer.ActiveCharacter.ID == _data.PlayerID)
			{
				//paperdoll requested for main player, all info should be up to date
				c = World.Instance.MainPlayer.ActiveCharacter;
				Array.Copy(_data.Paperdoll.ToArray(), c.PaperDoll, (int) EquipLocation.PAPERDOLL_MAX);
			}
			else
			{
				if ((c = World.Instance.ActiveMapRenderer.GetOtherPlayerByID(_data.PlayerID)) != null)
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
			World.Instance.ActiveMapRenderer.OnDoorOpened(x, y);
		}

		private void _chestOpen(ChestData data)
		{
			if (EOChestDialog.Instance == null || data.X != EOChestDialog.Instance.CurrentChestX ||
			    data.Y != EOChestDialog.Instance.CurrentChestY)
				return;

			EOChestDialog.Instance.InitializeItems(data.Items);
		}

		private void _chestAgree(ChestData data)
		{
			if (EOChestDialog.Instance != null)
				EOChestDialog.Instance.InitializeItems(data.Items);
		}

		private void _chestAddItem(short id, int amount, byte weight, byte maxWeight, ChestData data)
		{
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amount, weight, maxWeight);
			EOChestDialog.Instance.InitializeItems(data.Items);
			m_game.Hud.RefreshStats();
		}

		private void _chestGetItem(short id, int amount, byte weight, byte maxWeight, ChestData data)
		{
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amount, weight, maxWeight);
			EOChestDialog.Instance.InitializeItems(data.Items);
			m_game.Hud.RefreshStats();
		}

		private void _showPingTime(int timeout)
		{
			m_game.Hud.AddChat(ChatTabs.Local, "System", string.Format("[x] Current ping to the server is: {0} ms.", timeout), ChatType.LookingDude);
		}

		private void _showFindCommandResult(bool online, bool sameMap, string charName)
		{
			if (charName.Length == 0) return;

			string lastPart;
			if (online && !sameMap)
				lastPart = World.GetString(DATCONST2.STATUS_LABEL_IS_ONLINE_IN_THIS_WORLD);
			else if (online)
				lastPart = World.GetString(DATCONST2.STATUS_LABEL_IS_ONLINE_SAME_MAP);
			else
				lastPart = World.GetString(DATCONST2.STATUS_LABEL_IS_ONLINE_NOT_FOUND);

			m_game.Hud.AddChat(ChatTabs.Local,
				"System",
				string.Format("{0} " + lastPart, char.ToUpper(charName[0]) + charName.Substring(1)),
				ChatType.LookingDude);
		}

		private void _playerFace(short playerId, EODirection dir)
		{
			World.Instance.ActiveMapRenderer.OtherPlayerFace(playerId, dir);
		}

		private void _playerRecover(short hp, short tp)
		{
			World.Instance.MainPlayer.ActiveCharacter.Stats.SetHP(hp);
			World.Instance.MainPlayer.ActiveCharacter.Stats.SetTP(tp);
			m_game.Hud.RefreshStats();
		}

		private void _recoverReply(int exp, short karma, byte level)
		{
			World.Instance.MainPlayer.ActiveCharacter.Stats.exp = exp;
			World.Instance.MainPlayer.ActiveCharacter.Stats.karma = karma;
			if (level > 0)
				World.Instance.MainPlayer.ActiveCharacter.Stats.level = level;
			m_game.Hud.RefreshStats();
		}

		private void _statsList(DisplayStats _data)
		{

			CharStatData localStats = World.Instance.MainPlayer.ActiveCharacter.Stats;
			if (_data.IsStatsData)
				localStats.statpoints = _data.StatPoints;
			else
				World.Instance.MainPlayer.ActiveCharacter.Class = _data.Class;
			localStats.SetStr(_data.Str);
			localStats.SetInt(_data.Int);
			localStats.SetWis(_data.Wis);
			localStats.SetAgi(_data.Agi);
			localStats.SetCon(_data.Con);
			localStats.SetCha(_data.Cha);
			localStats.SetMaxHP(_data.MaxHP);
			localStats.SetMaxTP(_data.MaxTP);
			localStats.SetSP(_data.MaxSP);
			localStats.SetMaxSP(_data.MaxSP);
			World.Instance.MainPlayer.ActiveCharacter.MaxWeight = _data.MaxWeight;
			localStats.SetMinDam(_data.MinDam);
			localStats.SetMaxDam(_data.MaxDam);
			localStats.SetAccuracy(_data.Accuracy);
			localStats.SetEvade(_data.Evade);
			localStats.SetArmor(_data.Armor);
			m_game.Hud.RefreshStats();
		}

		private void _playerHeal(short playerid, int healamount, byte percenthealth)
		{
			World.Instance.ActiveMapRenderer.OtherPlayerHeal(playerid, healamount, percenthealth);
		}

		private void _getItemFromMap(short uid, short id, int amountTaken, byte weight, byte maxWeight)
		{

			if (uid != 0) //$si command has uid of 0 since we're creating a new item from nothing
			{
				World.Instance.ActiveMapRenderer.UpdateMapItemAmount(uid, amountTaken);
			}

			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amountTaken, weight, maxWeight, true);

			ItemRecord rec = World.Instance.EIF.GetItemRecordByID(id);
			m_game.Hud.AddChat(ChatTabs.System, "", string.Format("{0} {1} {2}", World.GetString(DATCONST2.STATUS_LABEL_ITEM_PICKUP_YOU_PICKED_UP), amountTaken, rec.Name), ChatType.UpArrow);
			m_game.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, DATCONST2.STATUS_LABEL_ITEM_PICKUP_YOU_PICKED_UP, string.Format(" {0} {1}", amountTaken, rec.Name));
		}

		private void _junkItem(short id, int amountRemoved, int amountRemaining, byte weight, byte maxWeight)
		{
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amountRemaining, weight, maxWeight);

			ItemRecord rec = World.Instance.EIF.GetItemRecordByID(id);
			m_game.Hud.AddChat(ChatTabs.System, "", string.Format("{0} {1} {2}", World.GetString(DATCONST2.STATUS_LABEL_ITEM_JUNK_YOU_JUNKED), amountRemoved, rec.Name), ChatType.DownArrow);
			m_game.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, DATCONST2.STATUS_LABEL_ITEM_JUNK_YOU_JUNKED, string.Format(" {0} {1}", amountRemoved, rec.Name));
		}

		private void _dropItem(int characterAmount, byte weight, byte maxWeight, MapItem item)
		{
			World.Instance.ActiveMapRenderer.AddMapItem(item);
			if (characterAmount >= 0) //will be -1 when another player drops
			{
				World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(item.id, characterAmount, weight, maxWeight);

				ItemRecord rec = World.Instance.EIF.GetItemRecordByID(item.id);
				m_game.Hud.AddChat(ChatTabs.System, "",
						string.Format("{0} {1} {2}", World.GetString(DATCONST2.STATUS_LABEL_ITEM_DROP_YOU_DROPPED), item.amount, rec.Name),
						ChatType.DownArrow);
				m_game.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, DATCONST2.STATUS_LABEL_ITEM_DROP_YOU_DROPPED,
						string.Format(" {0} {1}", item.amount, rec.Name));
			}
		}

		private void _useItem(ItemUseData data)
		{
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(data.ItemID, data.CharacterAmount, data.Weight, data.MaxWeight);
			switch (data.Type)
			{
				case ItemType.Teleport: /*Warp packet handles the rest!*/ break;
				case ItemType.Heal:
					{
						World.Instance.MainPlayer.ActiveCharacter.Stats.SetHP(data.HP);
						World.Instance.MainPlayer.ActiveCharacter.Stats.SetTP(data.TP);

						int percent = (int)Math.Round(100.0 * ((double)data.HP / World.Instance.MainPlayer.ActiveCharacter.Stats.maxhp));

						if (data.HPGain > 0)
							World.Instance.ActiveCharacterRenderer.SetDamageCounterValue(data.HPGain, percent, true);
						m_game.Hud.RefreshStats();
					}
					break;
				case ItemType.HairDye:
					{
						World.Instance.MainPlayer.ActiveCharacter.RenderData.SetHairColor(data.HairColor);
					}
					break;
				case ItemType.Beer:
					World.Instance.ActiveCharacterRenderer.MakeDrunk();
					m_game.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_WARNING, DATCONST2.STATUS_LABEL_ITEM_USE_DRUNK);
					break;
				case ItemType.EffectPotion:
					{
						//World.Instance.ActiveCharacterRenderer.ShowEffect(data.EffectID);
						//todo: get effects working
					}
					break;
				case ItemType.CureCurse:
					{
						//actually remove the item(s) from the main character
						Character c = World.Instance.MainPlayer.ActiveCharacter;
						for (int i = 0; i < (int)EquipLocation.PAPERDOLL_MAX; ++i)
						{
							int nextID = c.PaperDoll[i];
							if (nextID > 0 && World.Instance.EIF.GetItemRecordByID(nextID).Special == ItemSpecial.Cursed)
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
						s.SetMaxHP(data.CureStats.MaxHP);
						s.SetMaxTP(data.CureStats.MaxTP);
						s.SetStr(data.CureStats.Str);
						s.SetInt(data.CureStats.Int);
						s.SetWis(data.CureStats.Wis);
						s.SetAgi(data.CureStats.Agi);
						s.SetCon(data.CureStats.Con);
						s.SetCha(data.CureStats.Cha);
						s.SetMinDam(data.CureStats.MinDam);
						s.SetMaxDam(data.CureStats.MaxDam);
						s.SetAccuracy(data.CureStats.Accuracy);
						s.SetEvade(data.CureStats.Evade);
						s.SetArmor(data.CureStats.Armor);
						m_game.Hud.RefreshStats();
					}
					break;
				case ItemType.EXPReward:
					{
						CharStatData s = World.Instance.MainPlayer.ActiveCharacter.Stats;
						if (s.level < data.RewardStats.Level)
						{
							//level up!
							World.Instance.MainPlayer.ActiveCharacter.Emote(Emote.LevelUp);
							World.Instance.ActiveCharacterRenderer.PlayerEmote();
							s.level = data.RewardStats.Level;
						}
						s.exp = data.RewardStats.Exp;
						s.statpoints = data.RewardStats.StatPoints;
						s.skillpoints = data.RewardStats.SkillPoints;
						s.maxhp = data.RewardStats.MaxHP;
						s.maxtp = data.RewardStats.MaxTP;
						s.maxsp = data.RewardStats.MaxSP;
					}
					break;
			}
		}

		private void _removeItemFromMap(short itemuid)
		{
			World.Instance.ActiveMapRenderer.RemoveMapItem(itemuid);
		}

		private void _mapMutate()
		{
			if (File.Exists("maps\\00000.emf"))
			{
				string fmt = string.Format("maps\\{0,5:D5}.emf", World.Instance.MainPlayer.ActiveCharacter.CurrentMap);
				if (File.Exists(fmt))
					File.Delete(fmt);
				File.Move("maps\\00000.emf", fmt);
				World.Instance.Remap();
			}
			else
				throw new FileNotFoundException("Unable to remap the file, something broke");
		}

		private void _npcWalk(byte index, byte x, byte y, EODirection dir)
		{
			World.Instance.ActiveMapRenderer.NPCWalk(index, x, y, dir);
		}

		private void _npcAttack(byte index, bool dead, EODirection dir, short id, int damage, int health)
		{
			World.Instance.ActiveMapRenderer.NPCAttack(index, dead, dir, id, damage, health);
		}

		private void _npcChat(byte index, string message)
		{
			World.Instance.ActiveMapRenderer.RenderChatMessage(TalkType.NPC, index, message, ChatType.Note);
		}

		private void _npcLeaveView(byte index, int damage)
		{
			World.Instance.ActiveMapRenderer.RemoveOtherNPC(index, damage);
		}

		private void _npcKilled(int newExp)
		{
			int expDif = newExp - World.Instance.MainPlayer.ActiveCharacter.Stats.exp;
			World.Instance.MainPlayer.ActiveCharacter.GainExp(expDif);
			m_game.Hud.RefreshStats();

			m_game.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, DATCONST2.STATUS_LABEL_YOU_GAINED_EXP, string.Format(" {0} EXP", expDif));
			m_game.Hud.AddChat(ChatTabs.System, "", string.Format("{0} {1} EXP", World.GetString(DATCONST2.STATUS_LABEL_YOU_GAINED_EXP), expDif), ChatType.Star);
		}

		private void _npcTakeDamage(byte npcIndex, short fromPlayerID, EODirection fromDirection, int damageToNPC, int npcPctHealth)
		{
			World.Instance.ActiveMapRenderer.NPCTakeDamage(npcIndex, fromPlayerID, fromDirection, damageToNPC, npcPctHealth);
		}

		private void _playerLevelUp(LevelUpStats _stats)
		{
			World.Instance.MainPlayer.ActiveCharacter.Emote(Emote.LevelUp);
			World.Instance.ActiveCharacterRenderer.PlayerEmote();

			CharStatData stats = World.Instance.MainPlayer.ActiveCharacter.Stats;
			stats.level = _stats.Level;
			stats.statpoints = _stats.StatPoints;
			stats.skillpoints = _stats.SkillPoints;
			stats.SetMaxHP(_stats.MaxHP);
			stats.SetMaxTP(_stats.MaxTP);
			stats.SetMaxSP(_stats.MaxSP);
			m_game.Hud.RefreshStats();
		}

		private void _chatByPlayerID(TalkType type, int id, string message)
		{
			World.Instance.ActiveMapRenderer.RenderChatMessage(type, id, message, type == TalkType.Party ? ChatType.PlayerPartyDark : ChatType.SpeechBubble);
		}

		private void _chatByPlayerName(TalkType type, string name, string msg)
		{
			switch (type)
			{
				//invalid types
				case TalkType.Local:
				case TalkType.Party:
					break;
				case TalkType.PM:
					m_game.Hud.AddChat(ChatTabs.Local, name, msg, ChatType.Note, ChatColor.PM);
					ChatTabs tab = m_game.Hud.GetPrivateChatTab(name);
					m_game.Hud.AddChat(tab, name, msg, ChatType.Note);
					break;
				case TalkType.Global: m_game.Hud.AddChat(ChatTabs.Global, name, msg, ChatType.GlobalAnnounce); break;
				case TalkType.Guild: m_game.Hud.AddChat(ChatTabs.Group, name, msg); break;
				case TalkType.Server:
					m_game.Hud.AddChat(ChatTabs.Local, World.GetString(DATCONST2.STRING_SERVER), msg, ChatType.Exclamation, ChatColor.Server);
					m_game.Hud.AddChat(ChatTabs.Global, World.GetString(DATCONST2.STRING_SERVER), msg, ChatType.Exclamation, ChatColor.ServerGlobal);
					m_game.Hud.AddChat(ChatTabs.System, "", msg, ChatType.Exclamation, ChatColor.Server);
					break;
				case TalkType.Admin:
					m_game.Hud.AddChat(ChatTabs.Group, name, msg, ChatType.HGM, ChatColor.Admin);
					break;
				case TalkType.Announce:
					World.Instance.ActiveMapRenderer.MakeSpeechBubble(null, msg, false);
					m_game.Hud.AddChat(ChatTabs.Local, name, msg, ChatType.GlobalAnnounce, ChatColor.ServerGlobal);
					m_game.Hud.AddChat(ChatTabs.Global, name, msg, ChatType.GlobalAnnounce, ChatColor.ServerGlobal);
					m_game.Hud.AddChat(ChatTabs.Group, name, msg, ChatType.GlobalAnnounce, ChatColor.ServerGlobal);
					break;
			}
		}

		private void _pmRecipientNotFound(string name)
		{
			m_game.Hud.PrivatePlayerNotFound(name);
		}

		private void _playerMuted(string adminName)
		{
			string message = World.GetString(DATCONST2.CHAT_MESSAGE_MUTED_BY) + " " + adminName;
			m_game.Hud.AddChat(ChatTabs.Local, World.GetString(DATCONST2.STRING_SERVER), message, ChatType.Exclamation, ChatColor.Server);
			m_game.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, "" + Constants.MuteDefaultTimeMinutes, DATCONST2.STATUS_LABEL_MINUTES_MUTED);
			m_game.Hud.SetMuted();
		}

		private void _bankOpen(int gold, int upgrades)
		{
			if (EOBankAccountDialog.Instance == null) return;
			EOBankAccountDialog.Instance.AccountBalance = string.Format("{0}", gold);
			EOBankAccountDialog.Instance.LockerUpgrades = upgrades;
		}

		private void _bankChange(int gold, int bankGold)
		{
			if (EOBankAccountDialog.Instance == null) return;

			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(1, gold);
			EOBankAccountDialog.Instance.AccountBalance = string.Format("{0}", bankGold);
		}

		private void _shopOpen(int shopid, string name, List<ShopItem> tradeitems, List<CraftItem> craftitems)
		{
			if (EOShopDialog.Instance == null) return;
			EOShopDialog.Instance.SetShopData(shopid, name, tradeitems, craftitems);
		}

		private void _shopTrade(int gold, short itemID, int amount, byte weight, byte maxWeight, bool isBuy)
		{
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(1, gold);
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(itemID, amount, weight, maxWeight, isBuy);
		}

		private void _shopCraft(short id, byte weight, byte maxWeight, List<InventoryItem> ingredients)
		{
			Character c = World.Instance.MainPlayer.ActiveCharacter;
			c.UpdateInventoryItem(id, 1, weight, maxWeight, true);
			foreach (var ingred in ingredients)
				c.UpdateInventoryItem(ingred.id, ingred.amount);
		}

		private void _lockerOpen(byte x, byte y, List<InventoryItem> items)
		{
			if (EOLockerDialog.Instance == null || EOLockerDialog.Instance.X != x || EOLockerDialog.Instance.Y != y)
				return;
			EOLockerDialog.Instance.SetLockerData(items);
		}

		private void _lockerItemChange(short id, int amount, byte weight, byte maxWeight, bool existingAmount, List<InventoryItem> items)
		{
			if (EOLockerDialog.Instance == null) return;
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(id, amount, weight, maxWeight, existingAmount);
			EOLockerDialog.Instance.SetLockerData(items);
		}

		private void _lockerUpgrade(int remaining, byte upgrades)
		{
			if (EOBankAccountDialog.Instance == null) return;
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(1, remaining);
			EOBankAccountDialog.Instance.LockerUpgrades = upgrades;
			m_game.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, DATCONST2.STATUS_LABEL_LOCKER_SPACE_INCREASED);
		}

		private void _playerEmote(short playerID, Emote emote)
		{
			if (playerID != World.Instance.MainPlayer.ActiveCharacter.ID)
				World.Instance.ActiveMapRenderer.OtherPlayerEmote(playerID, emote);
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
			EODialog.Show(name + " ",
				   type == PartyRequestType.Join ? DATCONST1.PARTY_GROUP_REQUEST_TO_JOIN : DATCONST1.PARTY_GROUP_SEND_INVITATION,
				   XNADialogButtons.OkCancel, EODialogStyle.SmallDialogSmallHeader,
				   (o, e) =>
				   {
					   if (e.Result == XNADialogResult.OK)
					   {
						   if (!m_packetAPI.PartyAcceptRequest(type, id))
							   m_game.LostConnectionDialog();
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
			EODialog.Show(char.ToUpper(name[0]) + name.Substring(1) + " ", DATCONST1.TRADE_REQUEST, XNADialogButtons.OkCancel,
					EODialogStyle.SmallDialogSmallHeader, (o, e) =>
					{
						if (e.Result == XNADialogResult.OK && !m_packetAPI.TradeAcceptRequest(playerID))
							m_game.LostConnectionDialog();
					});
		}

		private void _tradeOpen(short p1, string p1name, short p2, string p2name)
		{
			EOTradeDialog dlg = new EOTradeDialog(m_packetAPI);
			dlg.InitPlayerInfo(p1, p1name, p2, p2name);

			string otherName;
			if (p1 == World.Instance.MainPlayer.ActiveCharacter.ID)
				otherName = p2name;
			else if (p2 == World.Instance.MainPlayer.ActiveCharacter.ID)
				otherName = p1name;
			else
				throw new ArgumentException("Invalid player ID for this trade session!", "p1");

			m_game.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_TRADE_YOU_ARE_TRADING_WITH,
					otherName + " " + World.GetString(DATCONST2.STATUS_LABEL_DRAG_AND_DROP_ITEMS));
		}

		private void _tradeCancel(short otherPlayerID)
		{
			if (EOTradeDialog.Instance == null) return;
			EOTradeDialog.Instance.Close(XNADialogResult.NO_BUTTON_PRESSED);
			m_game.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_ACTION, DATCONST2.STATUS_LABEL_TRADE_ABORTED);
		}

		private void _tradeRemotePlayerAgree(short otherPlayerID, bool agree)
		{
			if (EOTradeDialog.Instance == null) return;
			EOTradeDialog.Instance.SetPlayerAgree(false, agree);
		}

		private void _tradeSetLocalPlayerAgree(bool agree)
		{
			if (EOTradeDialog.Instance == null) return;
			EOTradeDialog.Instance.SetPlayerAgree(true, agree);
		}

		private void _tradeOfferUpdate(short id1, List<InventoryItem> items1, short id2, List<InventoryItem> items2)
		{
			if (EOTradeDialog.Instance == null) return;
			EOTradeDialog.Instance.SetPlayerItems(id1, items1);
			EOTradeDialog.Instance.SetPlayerItems(id2, items2);
		}

		private void _tradeCompleted(short id1, List<InventoryItem> items1, short id2, List<InventoryItem> items2)
		{
			if (EOTradeDialog.Instance == null) return;
			EOTradeDialog.Instance.CompleteTrade(id1, items1, id2, items2);
		}

		private void _skillmasterOpen(SkillmasterData data)
		{
			if (EOSkillmasterDialog.Instance != null)
				EOSkillmasterDialog.Instance.SetSkillmasterData(data);
		}

		private void _statskillLearnError(SkillMasterReply reply, short id)
		{
			switch (reply)
			{
				//not sure if this will ever actually be sent because client validates data before trying to learn a skill
				case SkillMasterReply.ErrorWrongClass:
					EODialog.Show(DATCONST1.SKILL_LEARN_WRONG_CLASS, " " + ((ClassRecord)World.Instance.ECF.Data[id]).Name + "!", XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
					break;
				case SkillMasterReply.ErrorRemoveItems:
					EODialog.Show(DATCONST1.SKILL_RESET_CHARACTER_CLEAR_PAPERDOLL, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
					break;
			}
		}

		private void _statskillLearnSpellSuccess(short id, int remaining)
		{
			World.Instance.MainPlayer.ActiveCharacter.Spells.Add(new CharacterSpell { id = id, level = 0 });
			if (EOSkillmasterDialog.Instance != null)
				EOSkillmasterDialog.Instance.RemoveSkillByIDFromLearnList(id);
			World.Instance.MainPlayer.ActiveCharacter.UpdateInventoryItem(1, remaining);
		}

		private void _statskillForgetSpell(short id)
		{
			World.Instance.MainPlayer.ActiveCharacter.Spells.RemoveAll(_spell => _spell.id == id);
			EODialog.Show(DATCONST1.SKILL_FORGET_SUCCESS, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
		}

		private void _statskillReset(StatResetData data)
		{
			Character c;
			(c = World.Instance.MainPlayer.ActiveCharacter).Spells.Clear();
			EODialog.Show(DATCONST1.SKILL_RESET_CHARACTER_COMPLETE, XNADialogButtons.Ok, EODialogStyle.SmallDialogSmallHeader);
			c.Stats.statpoints = data.StatPoints;
			c.Stats.skillpoints = data.SkillPoints;
			c.Stats.SetHP(data.HP);
			c.Stats.SetMaxHP(data.MaxHP);
			c.Stats.SetTP(data.TP);
			c.Stats.SetMaxTP(data.MaxTP);
			c.Stats.SetMaxSP(data.MaxSP);
			c.Stats.SetStr(data.Str);
			c.Stats.SetInt(data.Int);
			c.Stats.SetWis(data.Wis);
			c.Stats.SetAgi(data.Agi);
			c.Stats.SetCon(data.Con);
			c.Stats.SetCha(data.Cha);
			c.Stats.SetMinDam(data.MinDam);
			c.Stats.SetMaxDam(data.MaxDam);
			c.Stats.SetAccuracy(data.Accuracy);
			c.Stats.SetEvade(data.Evade);
			c.Stats.SetArmor(data.Armor);
			m_game.Hud.RefreshStats();
		}
	}
}
