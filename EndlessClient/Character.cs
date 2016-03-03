// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;
using EndlessClient.Dialogs;
using EndlessClient.Rendering;
using EOLib;
using EOLib.Data.BLL;
using EOLib.Data.Map;
using EOLib.IO;
using EOLib.Net.API;
using Microsoft.Xna.Framework;
using XNAControls;

namespace EndlessClient
{
	/// <summary>
	/// This is data used to render the character in CharacterRenderer.cs
	/// The values represented here are for loading GFX and do NOT represent IDs of items, etc.
	/// </summary>
	public class CharRenderData
	{
		private readonly object frameLocker = new object();
		public string name;
		public int id;
		public byte level, hairstyle, haircolor, race, admin;
		/// <summary>
		/// 0 == female and 1 == male
		/// </summary>
		public byte gender;
		public short boots, armor, hat, shield, weapon;

		public int walkFrame, attackFrame, emoteFrame = -1;
		
		public EODirection facing;
		public SitState sitting;
		public bool hidden;
		public bool update;
		public bool hairNeedRefresh;
		public bool dead;

		public Emote emote;

		public CharRenderData() { name = ""; }

		public void SetHairColor(byte color) { haircolor = color; update = true; }
		public void SetHairStyle(byte style) { hairstyle = style; update = true; }
		public void SetRace(byte newrace) { race = newrace; update = true; }
		public void SetGender(byte newgender) { gender = newgender; update = true; }

		public void SetDirection(EODirection direction) { facing = direction; update = true; }
		public void SetSitting(SitState sits) { sitting = sits; update = true; }
		public void SetHidden(bool hiding) { hidden = hiding; update = true; }

		public void SetShield(short newshield) { shield = newshield; update = true; }
		public void SetArmor(short newarmor) { armor = newarmor; update = true; }
		public void SetWeapon(short newweap) { weapon = newweap; update = true; }

		public void SetHat(short newhat)
		{
			if (hat != 0 && newhat == 0)
				hairNeedRefresh = true;
			hat = newhat; update = true;
		}
		public void SetBoots(short newboots) { boots = newboots; update = true; }

		public void SetWalkFrame(int wf)     { lock (frameLocker) walkFrame = wf; update = true; }
		public void SetAttackFrame(int af)   { lock (frameLocker) attackFrame = af; update = true; }
		public void SetEmoteFrame(int ef) { lock (frameLocker) emoteFrame = ef; update = true; }
		public void SetUpdate(bool shouldUpdate) { update = shouldUpdate; }
		public void SetHairNeedRefresh(bool shouldRefresh) { hairNeedRefresh = shouldRefresh; }
		
		public void SetDead(bool isDead) { dead = isDead; update = true; }
		public void SetEmote(Emote which) { emote = which; update = true; }
	}

	/// <summary>
	/// This is data used to track the stats of a player as a single, logically cohesive unit
	/// </summary>
	public class CharStatData
	{
		public byte Level { get; set; }
		public int Experience { get; set; }
		public int Usage { get; set; }

		public short HP { get; set; }
		public short MaxHP { get; set; }
		public short TP { get; set; }
		public short MaxTP { get; set; }
		public short SP { get; set; }
		public short MaxSP { get; set; }

		public short StatPoints { get; set; }
		public short SkillPoints { get; set; }
		public short Karma { get; set; }
		public short MinDam { get; set; }
		public short MaxDam { get; set; }
		public short Accuracy { get; set; }
		public short Evade { get; set; }
		public short Armor { get; set; }

		public short Str { get; set; }
		public short Int { get; set; }
		public short Wis { get; set; }
		public short Agi { get; set; }
		public short Con { get; set; }
		public short Cha { get; set; }
	}

	/// <summary>
	/// Note: since a lot of calls are made asynchronously, there could be issues with
	///	<para>data races to the properties that are here. However, since it is updating</para>
	/// <para>and redrawing so fast, I don't think it will matter all that much.</para>
	/// </summary>
	public class Character
	{
		public int ID { get; private set; }

		/// <summary>
		/// x*32 - y*32 + ViewAdjustX
		/// </summary>
		public int OffsetX
		{
			get { return X*32 - Y*32 + ViewAdjustX; }
		}

		/// <summary>
		/// x*16 + y*16 + ViewAdjustY
		/// </summary>
		public int OffsetY
		{
			get { return X*16 + Y*16 + ViewAdjustY; }
		}

		public byte DestX { get; private set; }
		public byte DestY { get; private set; }
		public int ViewAdjustX { get; set; }
		public int ViewAdjustY { get; set; }
		public bool CanAttack { get { return Weight <= MaxWeight && Stats.SP > 0; } }

		public CharacterActionState State { get; private set; }

		//paperdoll info
		public string Name { get; set; }
		public string Title { get; set; }
		public string GuildName { get; set; }
// ReSharper disable UnusedAutoPropertyAccessor.Global
		public string GuildRankStr { get; set; }
		public byte Class { get; set; }
		public string PaddedGuildTag { get; set; }
// ReSharper restore UnusedAutoPropertyAccessor.Global
		public AdminLevel AdminLevel { get; set; }

		public byte Weight { get; set; }
		public byte MaxWeight { get; set; }
		public short[] PaperDoll { get; private set; }
		public List<InventoryItem> Inventory { get; private set; }
		public List<CharacterSpell> Spells { get; private set; }

		public CharStatData Stats { get; set; }
		public CharRenderData RenderData { get; private set; }

		public short CurrentMap { get; set; }
		public int X { get; private set; }
		public int Y { get; private set; }

		public int SelectedSpell { get; private set; }
		public bool PreparingSpell { get; private set; }
		public bool NeedsSpellTarget
		{
			get
			{
				var target = OldWorld.Instance.ESF.GetRecordByID((short) SelectedSpell).Target;
				return SelectedSpell > 0 &&
				       target == EOLib.IO.SpellTarget.Normal &&
				       SpellTarget == null;
			}
		}
		public DrawableGameComponent SpellTarget { get; private set; }

		public byte GuildRankNum { private get; set; }

		public int TodayExp { get; private set; }
		public int TodayBestKill { get; private set; }
		public int TodayLastKill { get; private set; }

		public bool IsDrunk { get; set; }

		public static string KarmaStringFromNum(int num)
		{
		   /* 0    - 100  = Demonic
			* 101  - 500  = Doomed
			* 501  - 750  = Cursed
			* 751  - 900  = Evil
			* 901  - 1099 = Neutral
			* 1100 - 1249 = Good
			* 1250 - 1499 = Blessed
			* 1500 - 1899 = Saint
			* 1900 - 2000 = Pure
			*/
			if (num >= 0)
			{
				if (num <= 100)
					return "Demonic";
				if (num <= 500)
					return "Doomed";
				if (num <= 750)
					return "Cursed";
				if (num <= 900)
					return "Evil";
				if (num <= 1099)
					return "Neutral";
				if (num <= 1249)
					return "Good";
				if (num <= 1499)
					return "Blessed";
				if (num <= 1899)
					return "Saint";
				if (num <= 2000)
					return "Pure";
			}

			throw new ArgumentOutOfRangeException("num", num, "Karma values must be between 0-2000");
		}

		private readonly PacketAPI m_packetAPI;

		public Character()
		{
			//mock all members with non-null fields
			//PacketAPI cannot be mocked...
			Inventory = new List<InventoryItem>();
			Spells = new List<CharacterSpell>();
			PaperDoll = new short[(int)EquipLocation.PAPERDOLL_MAX];

			Stats = new CharStatData();
			RenderData = new CharRenderData();

			Name = PaddedGuildTag = GuildName = GuildRankStr = "";
		}

		public Character(PacketAPI api, int id, CharRenderData data)
		{
			m_packetAPI = api;
			ID = id;
			RenderData = data ?? new CharRenderData();

			Inventory = new List<InventoryItem>();
			Spells = new List<CharacterSpell>();
			PaperDoll = new short[(int)EquipLocation.PAPERDOLL_MAX];
		}

		//constructs a character from a packet sent from the server
		public Character(PacketAPI api, CharacterData data)
		{
			//initialize lists
			m_packetAPI = api;

			Inventory = new List<InventoryItem>();
			Spells = new List<CharacterSpell>();
			PaperDoll = new short[(int)EquipLocation.PAPERDOLL_MAX];

			Name = data.Name;
			
			ID = data.ID;
			CurrentMap = data.Map;
			X = data.X;
			Y = data.Y;

			PaddedGuildTag = data.GuildTag;

			RenderData = new CharRenderData
			{
				facing = data.Direction,
				level = data.Level,
				gender = data.Gender,
				hairstyle = data.HairStyle,
				haircolor = data.HairColor,
				race = data.Race
			};

			Stats = new CharStatData
			{
				MaxHP = data.MaxHP,
				HP = data.HP,
				MaxTP = data.MaxTP,
				TP = data.TP
			};

			EquipItem(ItemType.Boots, 0, data.Boots, true);
			EquipItem(ItemType.Armor, 0, data.Armor, true);
			EquipItem(ItemType.Hat, 0, data.Hat, true);
			EquipItem(ItemType.Shield, 0, data.Shield, true);
			EquipItem(ItemType.Weapon, 0, data.Weapon, true);
			
			RenderData.SetSitting(data.Sitting);
			RenderData.SetHidden(data.Hidden);
		}

		public void UnequipItem(ItemType type, byte subLoc)
		{
			EquipItem(type, 0, 0, true, (sbyte)subLoc);
		}

		public bool EquipItem(ItemType type, short id = 0, short graphic = 0, bool rewrite = false, sbyte subloc = -1)
		{
			switch (type)
			{
				case ItemType.Weapon:
					if (PaperDoll[(int)EquipLocation.Weapon] != 0 && !rewrite) return false;
					PaperDoll[(int) EquipLocation.Weapon] = id;
					RenderData.SetWeapon(graphic);
					break;
				case ItemType.Shield:
					if (PaperDoll[(int)EquipLocation.Shield] != 0 && !rewrite) return false;
					PaperDoll[(int) EquipLocation.Shield] = id;
					RenderData.SetShield(graphic);
					break;
				case ItemType.Armor:
					if (PaperDoll[(int)EquipLocation.Armor] != 0 && !rewrite) return false;
					PaperDoll[(int) EquipLocation.Armor] = id;
					RenderData.SetArmor(graphic);
					break;
				case ItemType.Hat:
					if (PaperDoll[(int)EquipLocation.Hat] != 0 && !rewrite) return false;
					PaperDoll[(int) EquipLocation.Hat] = id;
					RenderData.SetHat(graphic);
					break;
				case ItemType.Boots:
					if (PaperDoll[(int)EquipLocation.Boots] != 0 && !rewrite) return false;
					PaperDoll[(int) EquipLocation.Boots] = id;
					RenderData.SetBoots(graphic);
					break;
				case ItemType.Gloves:
					if (PaperDoll[(int)EquipLocation.Gloves] != 0 && !rewrite) return false;
					PaperDoll[(int) EquipLocation.Gloves] = id;
					break;
				case ItemType.Accessory:
					if (PaperDoll[(int)EquipLocation.Accessory] != 0 && !rewrite) return false;
					PaperDoll[(int) EquipLocation.Accessory] = id;
					break;
				case ItemType.Belt:
					if (PaperDoll[(int)EquipLocation.Belt] != 0 && !rewrite) return false;
					PaperDoll[(int) EquipLocation.Belt] = id;
					break;
				case ItemType.Necklace:
					if (PaperDoll[(int) EquipLocation.Necklace] != 0 && !rewrite) return false;
					PaperDoll[(int) EquipLocation.Necklace] = id;
					break;
				case ItemType.Ring:
					if (subloc != -1)
					{
						//subloc was explicitly specified
						if (!rewrite && PaperDoll[(int) EquipLocation.Ring1 + subloc] != 0)
							return false;
						PaperDoll[(int) EquipLocation.Ring1 + subloc] = id;
					}
					else
					{
						if (PaperDoll[(int) EquipLocation.Ring1] != 0 && PaperDoll[(int) EquipLocation.Ring2] != 0 && !rewrite)
							return false;
						if (PaperDoll[(int) EquipLocation.Ring1] != 0 && PaperDoll[(int) EquipLocation.Ring2] == 0)
							PaperDoll[(int) EquipLocation.Ring2] = id;
						else
							PaperDoll[(int) EquipLocation.Ring1] = id;
					}
					break;
				case ItemType.Armlet:
					if (subloc != -1)
					{
						//subloc was explicitly specified
						if (!rewrite && PaperDoll[(int) EquipLocation.Armlet1 + subloc] != 0)
							return false;
						PaperDoll[(int) EquipLocation.Armlet1 + subloc] = id;
					}
					else
					{
						if (PaperDoll[(int) EquipLocation.Armlet1] != 0 && PaperDoll[(int) EquipLocation.Armlet2] != 0 && !rewrite)
							return false;
						if (PaperDoll[(int) EquipLocation.Armlet1] != 0 && PaperDoll[(int) EquipLocation.Armlet2] == 0)
							PaperDoll[(int) EquipLocation.Armlet2] = id;
						else
							PaperDoll[(int) EquipLocation.Armlet1] = id;
					}
					break;
				case ItemType.Bracer:
					if (subloc != -1)
					{
						//subloc was explicitly specified
						if (!rewrite && PaperDoll[(int) EquipLocation.Bracer1 + subloc] != 0)
							return false;
						PaperDoll[(int) EquipLocation.Bracer1 + subloc] = id;
					}
					else
					{
						if (PaperDoll[(int) EquipLocation.Bracer1] != 0 && PaperDoll[(int) EquipLocation.Bracer2] != 0 && !rewrite)
							return false;
						if (PaperDoll[(int) EquipLocation.Bracer1] != 0 && PaperDoll[(int) EquipLocation.Bracer2] == 0)
							PaperDoll[(int) EquipLocation.Bracer2] = id;
						else
							PaperDoll[(int) EquipLocation.Bracer1] = id;
					}
					break;
				default:
					throw new ArgumentOutOfRangeException("type", "Invalid item type for equip!");
			}

			return true;
		}

		/// <summary>
		/// Used to apply changes from Welcome packet to existing character.
		/// </summary>
		/// <param name="newGuy">Changes to MainPlayer.ActiveCharacter, contained in a CharacterData object</param>
		/// <param name="copyPaperdoll">Set to true if paperdoll data from newGuy should be applied to this character</param>
		public void ApplyData(CharacterData newGuy, bool copyPaperdoll = true)
		{
			ID = newGuy.ID;
			Name = newGuy.Name;
			PaddedGuildTag = newGuy.GuildTag;
			if (copyPaperdoll)
			{
				//only set the doll graphic info in render data - don't change the paperdoll info!
				RenderData.SetBoots(newGuy.Boots);
				RenderData.SetArmor(newGuy.Armor);
				RenderData.SetHat(newGuy.Hat);
				RenderData.SetShield(newGuy.Shield);
				RenderData.SetWeapon(newGuy.Weapon);
			}
			Stats.HP = newGuy.HP;
			Stats.MaxHP = newGuy.MaxHP;
			Stats.TP = newGuy.TP;
			Stats.MaxTP = newGuy.MaxTP;

			RenderData.SetDirection(newGuy.Direction);
			RenderData.SetHairStyle(newGuy.HairStyle);
			RenderData.SetHairColor(newGuy.HairColor);
			RenderData.SetGender(newGuy.Gender);
			RenderData.SetRace(newGuy.Race);
			RenderData.SetSitting(newGuy.Sitting);
			RenderData.level = newGuy.Level;
			RenderData.SetWalkFrame(0);
			State = RenderData.sitting == SitState.Standing ? CharacterActionState.Standing : CharacterActionState.Sitting;
			CurrentMap = newGuy.Map;
			X = newGuy.X;
			Y = newGuy.Y;
			//GuildRankNum = newGuy.GuildRankNum; //todo: ???
		}

		/// <summary>
		/// Called from CharacterRenderer.Update (in case of MainPlayer pressing an arrow key) or Handlers.Walk (in case of another character walking)
		/// <para>The Character Renderer will automatically pick up that Walking == true and start a walk operation, limiting the character from walking again until it is complete</para>
		/// </summary>
		public void Walk(EODirection direction, byte destX, byte destY, bool admin)
		{
			if (State != CharacterActionState.Standing)
				return;

			if (this == OldWorld.Instance.MainPlayer.ActiveCharacter)
			{
				if(!m_packetAPI.PlayerWalk(direction, destX, destY, admin && AdminLevel != AdminLevel.Player))
					EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
			}
			else if (RenderData.facing != direction) //if a packet WALK_PLAYER was received: face them the right way first otherwise this will look weird
				RenderData.SetDirection(direction);

			DestX = destX;
			DestY = destY;
			State = CharacterActionState.Walking;
		}

		public void DoneWalking()
		{
			ViewAdjustX = 0;
			ViewAdjustY = 0;
			State = CharacterActionState.Standing;
			X = DestX;
			Y = DestY;
			RenderData.SetWalkFrame(0);
		}

		public void Attack(EODirection direction, byte x = 255, byte y = 255)
		{
			if (State != CharacterActionState.Standing) return;

			if (this == OldWorld.Instance.MainPlayer.ActiveCharacter)
			{
				//KS protection - vanilla eoserv does not support this!
				//Enabled client-side only, the official client does not support this
				var shouldSend = true;
				if (!(x == 255 && y == 255))
				{
					var ti = OldWorld.Instance.ActiveMapRenderer.GetTileInfo(x, y);
					if (ti.ReturnType == TileInfoReturnType.IsOtherNPC && 
						((NPC)ti.MapElement).Opponent != null &&
						((NPC)ti.MapElement).Opponent != this)
					{
						EOGame.Instance.Hud.SetStatusLabel(DATCONST2.STATUS_LABEL_TYPE_INFORMATION, DATCONST2.STATUS_LABEL_UNABLE_TO_ATTACK);
						shouldSend = false;
					}
				}

				if(shouldSend && !m_packetAPI.AttackUse(direction))
					EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
			}
			else if(RenderData.facing != direction)
				RenderData.SetDirection(direction);

			State = CharacterActionState.Attacking;
			Stats.SP--;
		}

		public void DoneAttacking()
		{
			State = CharacterActionState.Standing;
			RenderData.SetAttackFrame(0);
		}

		public void Emote(Emote whichEmote)
		{
			if (this == OldWorld.Instance.MainPlayer.ActiveCharacter &&
				whichEmote != EOLib.Net.API.Emote.LevelUp &&
				whichEmote != EOLib.Net.API.Emote.Trade)
			{
				if (m_packetAPI.ReportEmote(whichEmote))
					RenderData.SetEmote(whichEmote);
				else
					EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
			}

			RenderData.SetEmoteFrame(0);
			RenderData.SetEmote(whichEmote);
		}

		public void DoneEmote()
		{
			RenderData.SetEmoteFrame(-1);
		}

		public void Face(EODirection direction)
		{
			//send packet to server: update client side if send was successful
			if(m_packetAPI.FacePlayer(direction))
				RenderData.SetDirection(direction); //updates the data in the character renderer as well
			else
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
		}

		public void UpdateInventoryItem(short id, int characterAmount, bool add = false)
		{
			InventoryItem rec;
			if ((rec = Inventory.Find(item => item.id == id)).id == id)
			{
				InventoryItem newRec = new InventoryItem {amount = add ? characterAmount + rec.amount : characterAmount, id = id};
				if (!Inventory.Remove(rec))
					throw new Exception("Unable to remove from inventory!");
				if (newRec.amount > 0)
				{
					Inventory.Add(newRec);
				}
				if (this == OldWorld.Instance.MainPlayer.ActiveCharacter) EOGame.Instance.Hud.UpdateInventory(newRec);
			}
			else //if unequipping an item that isn't in the inventory yet
			{
				InventoryItem newRec = new InventoryItem {amount = characterAmount, id = id};
				Inventory.Add(newRec);
				if (this == OldWorld.Instance.MainPlayer.ActiveCharacter) EOGame.Instance.Hud.UpdateInventory(newRec);
			}
		}

		public void UpdateInventoryItem(short id, int characterAmount, byte characterWeight, byte characterMaxWeight, bool addToExistingAmount = false)
		{
			InventoryItem rec;
			if ((rec = Inventory.Find(item => item.id == id)).id == id)
			{
				InventoryItem newRec = new InventoryItem
				{
					amount = addToExistingAmount ? characterAmount + rec.amount : characterAmount,
					id = id
				};
				if (this == OldWorld.Instance.MainPlayer.ActiveCharacter)
				{
					//false when AddItem fails to find a good spot
					if (!EOGame.Instance.Hud.UpdateInventory(newRec))
					{
						EOMessageBox.Show(OldWorld.GetString(DATCONST2.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT),
							OldWorld.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING),
							XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
						return;
					}
				}

				//if we can hold it, update local inventory and weight stats
				if (!Inventory.Remove(rec))
					throw new Exception("Unable to remove from inventory!");
				if (newRec.amount > 0)
				{
					Inventory.Add(newRec);
				}
				Weight = characterWeight;
				MaxWeight = characterMaxWeight;
				if (this == OldWorld.Instance.MainPlayer.ActiveCharacter) EOGame.Instance.Hud.RefreshStats();
			}
			else
			{
				//for item_get/chest_get packets, the item may not be in the inventory yet
				InventoryItem newRec = new InventoryItem {amount = characterAmount, id = id};
				if (newRec.amount <= 0) return;
				
				Inventory.Add(newRec);
				if (this == OldWorld.Instance.MainPlayer.ActiveCharacter)
				{
					//false when AddItem fails to find a good spot
					if (!EOGame.Instance.Hud.UpdateInventory(newRec))
					{
						EOMessageBox.Show(OldWorld.GetString(DATCONST2.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT),
							OldWorld.GetString(DATCONST2.STATUS_LABEL_TYPE_WARNING),
							XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);
						return;
					}
				}
				Weight = characterWeight;
				MaxWeight = characterMaxWeight;
				if (this == OldWorld.Instance.MainPlayer.ActiveCharacter) EOGame.Instance.Hud.RefreshStats();
			}
		}

		public void SetDisplayItemsFromRenderData(CharRenderData newRenderData)
		{
			EquipItem(ItemType.Boots,  (short)(OldWorld.Instance.EIF.Data.SingleOrDefault(x => x.Type == ItemType.Boots  && x.DollGraphic == newRenderData.boots)  ?? new ItemRecord(0)).ID, newRenderData.boots,  true);
			EquipItem(ItemType.Armor,  (short)(OldWorld.Instance.EIF.Data.SingleOrDefault(x => x.Type == ItemType.Armor  && x.DollGraphic == newRenderData.armor)  ?? new ItemRecord(0)).ID, newRenderData.armor,  true);
			EquipItem(ItemType.Hat,    (short)(OldWorld.Instance.EIF.Data.SingleOrDefault(x => x.Type == ItemType.Hat    && x.DollGraphic == newRenderData.hat)    ?? new ItemRecord(0)).ID, newRenderData.hat,    true);
			EquipItem(ItemType.Shield, (short)(OldWorld.Instance.EIF.Data.SingleOrDefault(x => x.Type == ItemType.Shield && x.DollGraphic == newRenderData.shield) ?? new ItemRecord(0)).ID, newRenderData.shield, true);
			EquipItem(ItemType.Weapon, (short)(OldWorld.Instance.EIF.Data.SingleOrDefault(x => x.Type == ItemType.Weapon && x.DollGraphic == newRenderData.weapon) ?? new ItemRecord(0)).ID, newRenderData.weapon, true);
		}

		public void GainExp(int amount)
		{
			TodayLastKill = amount;
			if (amount > TodayBestKill)
				TodayBestKill = amount;
			TodayExp += amount;
			Stats.Experience += amount;
		}

		public void UpdateStatsAfterEquip(PaperdollEquipData data)
		{
			Stats.MaxHP = data.MaxHP;
			Stats.MaxTP = data.MaxTP;

			Stats.Str = data.Str;
			Stats.Int = data.Int;
			Stats.Wis = data.Wis;
			Stats.Agi = data.Agi;
			Stats.Con = data.Con;
			Stats.Cha = data.Cha;

			Stats.MinDam = data.MinDam;
			Stats.MaxDam = data.MaxDam;
			Stats.Accuracy = data.Accuracy;
			Stats.Evade = data.Evade;
			Stats.Armor = data.Armor;
		}

		/// <summary>
		/// Gets the permission of the current character to open the specified door
		/// </summary>
		/// <returns>Returns DoorSpec.Door if this character may open the door. Otherwise, returns the type of door restricting access</returns>
		public DoorSpec CanOpenDoor(DoorSpec door)
		{
			DoorSpec permission = door;

			ItemRecord rec;
			switch (door) //note - it would be nice to be able to send the Item IDs of the keys in the welcome packet or something
			{
				case DoorSpec.LockedCrystal:
					rec = OldWorld.Instance.EIF.Data.Single(_rec => _rec.Name != null && _rec.Name.ToLower() == "crystal key");
					break;
				case DoorSpec.LockedSilver:
					rec = OldWorld.Instance.EIF.Data.Single(_rec => _rec.Name != null && _rec.Name.ToLower() == "silver key");
					break;
				case DoorSpec.LockedWraith:
					rec = OldWorld.Instance.EIF.Data.Single(_rec => _rec.Name != null && _rec.Name.ToLower() == "wraith key");
					break;
				default:
					return permission;
			}

			if(rec != null && Inventory.FindIndex(_ii => _ii.id == rec.ID) >= 0)
				permission = DoorSpec.Door;
			else if (rec == null) //show a warning saying that this door is perma-locked. Non-standard pub files will cause this.
				EOMessageBox.Show(
					string.Format("Unable to find key for {0} in EIF. This door will never be opened!",
						Enum.GetName(typeof (DoorSpec), permission)), "Warning", XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);

			return permission;
		}

		public ChestKey CanOpenChest(MapChest chest)
		{
			ChestKey permission = chest.key;

			ItemRecord rec;
			switch (permission) //note - it would be nice to be able to send the Item IDs of the keys in the welcome packet or something
			{
				case ChestKey.Normal:
					rec = OldWorld.Instance.EIF.Data.Single(_rec => _rec.Name != null && _rec.Name.ToLower() == "normal key");
					break;
				case ChestKey.Crystal:
					rec = OldWorld.Instance.EIF.Data.Single(_rec => _rec.Name != null && _rec.Name.ToLower() == "crystal key");
					break;
				case ChestKey.Silver:
					rec = OldWorld.Instance.EIF.Data.Single(_rec => _rec.Name != null && _rec.Name.ToLower() == "silver key");
					break;
				case ChestKey.Wraith:
					rec = OldWorld.Instance.EIF.Data.Single(_rec => _rec.Name != null && _rec.Name.ToLower() == "wraith key");
					break;
				default:
					return permission;
			}

			if (rec != null && Inventory.FindIndex(_ii => _ii.id == rec.ID) >= 0)
				permission = ChestKey.None;
			else if (rec == null) //show a warning saying that this chest is perma-locked. Non-standard pub files will cause this.
				EOMessageBox.Show(
					string.Format("Unable to find key for {0} in EIF. This chest will never be opened!",
						Enum.GetName(typeof(ChestKey), permission)), "Warning", XNADialogButtons.Ok, EOMessageBoxStyle.SmallDialogSmallHeader);

			return permission;
		}

		public void SelectSpell(int id)
		{
			//right now this is just a setter for SelectedSpell
			if (SelectedSpell != id)
			{
				SelectedSpell = id;
			}
		}

		public void PrepareSpell(int id)
		{
			if (SelectedSpell <= 0)
				return;

			if (!m_packetAPI.PrepareCastSpell((short) id))
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();

			PreparingSpell = true;
		}

		public void CastSpell(int id)
		{
			if (SelectedSpell <= 0)
				return;

			SpellRecord data = OldWorld.Instance.ESF.GetRecordByID((short)id);
			bool result = false;
			switch (data.Target)
			{
				case EOLib.IO.SpellTarget.Normal:
					var targetAsNPC = SpellTarget as NPCRenderer;
					var targetAsChar = SpellTarget as CharacterRenderer;
					if (targetAsNPC != null)
						result = m_packetAPI.DoCastTargetSpell((short) id, true, targetAsNPC.NPC.Index);
					else if (targetAsChar != null)
						result = m_packetAPI.DoCastTargetSpell((short) id, false, (short) targetAsChar.Character.ID);
					break;
				case EOLib.IO.SpellTarget.Self:
					result = m_packetAPI.DoCastSelfSpell((short) id);
					break;
				case EOLib.IO.SpellTarget.Unknown1:
					throw new Exception("What even is this");
				case EOLib.IO.SpellTarget.Group:
					result = m_packetAPI.DoCastGroupSpell((short) id);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			SetSpellCastStart();

			if (!result)
				EOGame.Instance.DoShowLostConnectionDialogAndReturnToMainMenu();
		}

		public void SetSpellTarget(DrawableGameComponent target)
		{
			if (target != null && !(target is NPCRenderer || target is CharacterRenderer)) //don't set target when it isn't valid!
				return;

			SpellTarget = target;
		}

		public void CancelSpellPrepare()
		{
			PreparingSpell = false;
		}

		public void SetSpellCastStart()
		{
			PreparingSpell = false;
			State = CharacterActionState.SpellCast;
			RenderData.SetUpdate(true);
		}

		public void SetSpellCastComplete()
		{
			State = CharacterActionState.Standing;
			RenderData.SetUpdate(true);
		}
	}
}
