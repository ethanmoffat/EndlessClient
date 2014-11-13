using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using EOLib;
using EOLib.Data;
using Microsoft.Xna.Framework;

namespace EndlessClient
{
	public enum AdminLevel
	{
		Player,
		Guide,
		Guardian,
		GM,
		HGM
	}

	public enum SitState
	{
		Standing,
		Chair,
		Floor
	}

	public enum EquipLocation
	{
		Boots = 0,
		Accessory,
		Gloves,
		Belt,
		Armor,
		Necklace,
		Hat,
		Shield,
		Weapon,
		Ring1,
		Ring2,
		Armlet1,
		Armlet2,
		Bracer1,
		Bracer2,
		PAPERDOLL_MAX = 15
	}

	/// <summary>
	/// This is data used to render the character in EOCharacterRenderer.cs
	/// The values represented here are for loading GFX and do NOT represent IDs of items, etc.
	/// </summary>
	public class CharRenderData
	{
		private static readonly object walkFrameLocker = new object();
		public string name;
		public int id;
		public byte level, gender, hairstyle, haircolor, race, admin;
		public short boots, armor, hat, shield, weapon;

		public byte walkFrame;
		
		public EODirection facing;
		public SitState sitting;
		public bool hidden;
		public bool update;
		public bool hairNeedRefresh;

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

		public void SetWalkFrame(byte wf) { lock(walkFrameLocker) walkFrame = wf; update = true; }
		public void SetUpdate(bool shouldUpdate) { update = shouldUpdate; }
		public void SetHairNeedRefresh(bool shouldRefresh) { hairNeedRefresh = shouldRefresh; }
	}

	/// <summary>
	/// This is data used to track the stats of a player as a single, logically cohesive unit
	/// </summary>
	public struct CharStatData
	{
		public byte level;
		public int exp;
		public int usage;

		public short hp;
		public short maxhp;
		public short tp;
		public short maxtp;
		public short maxsp;

		public short statpoints;
		public short skillpoints;
		public short karma;
		public short mindam;
		public short maxdam;
		public short accuracy;
		public short evade;
		public short armor;

		public short disp_str;
		public short disp_int;
		public short disp_wis;
		public short disp_agi;
		public short disp_con;
		public short disp_cha;

		public CharStatData(CharStatData other)
		{
			level = other.level;
			exp = other.exp;
			usage = other.usage;
			hp = other.hp;
			maxhp = other.maxhp;
			tp = other.tp;
			maxtp = other.maxtp;
			maxsp = other.maxsp;
			statpoints = other.statpoints;
			skillpoints = other.skillpoints;
			karma = other.karma;
			mindam = other.mindam;
			maxdam = other.maxdam;
			accuracy = other.accuracy;
			evade = other.evade;
			armor = other.armor;
			disp_str = other.disp_str;
			disp_int = other.disp_int;
			disp_wis = other.disp_wis;
			disp_agi = other.disp_agi;
			disp_con = other.disp_con;
			disp_cha = other.disp_cha;
		}
	}

	/// <summary>
	/// Essentially just a key-value pair between an item and the amount of that item in the inventory
	/// </summary>
	public struct InventoryItem
	{
		public short id;
		public int amount;
	}

	public struct CharacterSpell
	{
		public short id;
		public short level;
	}

	/// <summary>
	/// Note: since a lot of calls are made asynchronously, there could be issues with
	///	<para>data races to the properties that are here. However, since it is updating</para>
	/// <para>and redrawing so fast, I don't think it will matter all that much.</para>
	/// </summary>
	public class Character
	{
		public int ID { get; private set; }

		public int OffsetX
		{
			get { return X*32 - Y*32 + ViewAdjustX; }
		}

		public int OffsetY
		{
			get { return X*16 + Y*16 + ViewAdjustY; }
		}

		private byte destx, desty;
		public int ViewAdjustX { get; set; }
		public int ViewAdjustY { get; set; }
		public bool Walking { get; private set; }

		//paperdoll info
		public string Name { get; set; }
		public string Title { get; set; }
		public string GuildName { get; set; }
		public string GuildRankStr { get; set; }
		public byte Class { get; set; }
		public string PaddedGuildTag { get; set; }
		public AdminLevel AdminLevel { get; set; }

		public byte Weight { get; set; }
		public byte MaxWeight { get; set; }
		public short[] PaperDoll { get; set; }
		public List<InventoryItem> Inventory { get; set; }
		public List<CharacterSpell> Spells { get; set; }

		public CharStatData Stats { get; set; }

		public CharRenderData RenderData { get; set; }
		public short CurrentMap { get; set; }
		public bool MapIsPk { get; set; }
		public int X { get; set; }
		public int Y { get; set; }

		public byte GuildRankNum { get; set; }
		
		public Character(int id, CharRenderData data)
		{
			ID = id;
			RenderData = data ?? new CharRenderData();

			Inventory = new List<InventoryItem>();
			Spells = new List<CharacterSpell>();
			PaperDoll = new short[(int)EquipLocation.PAPERDOLL_MAX];
		}

		//constructs a character from a packet sent from the server
		public Character(Packet pkt)
		{
			//initialize lists
			Inventory = new List<InventoryItem>();
			Spells = new List<CharacterSpell>();
			PaperDoll = new short[(int)EquipLocation.PAPERDOLL_MAX];

			Name = pkt.GetBreakString();
			if (Name.Length > 1)
				Name = char.ToUpper(Name[0]) + Name.Substring(1);
			
			ID = pkt.GetShort();
			CurrentMap = pkt.GetShort();
			X = pkt.GetShort();
			Y = pkt.GetShort();

			EODirection direction = (EODirection)pkt.GetChar();
			pkt.GetChar(); //value is always 6? unknown
			PaddedGuildTag = pkt.GetFixedString(3);

			RenderData = new CharRenderData
			{
				facing = direction,
				level = pkt.GetChar(),
				gender = pkt.GetChar(),
				hairstyle = pkt.GetChar(),
				haircolor = pkt.GetChar(),
				race = pkt.GetChar()
			};

			Stats = new CharStatData
			{
				maxhp = pkt.GetShort(),
				hp = pkt.GetShort(),
				maxtp = pkt.GetShort(),
				tp = pkt.GetShort()
			};

			RenderData.SetBoots(pkt.GetShort());
			pkt.Skip(3 * sizeof(short)); //other paperdoll data is 0'd out
			RenderData.SetArmor(pkt.GetShort());
			pkt.Skip(sizeof(short));
			RenderData.SetHat(pkt.GetShort());
			RenderData.SetShield(pkt.GetShort());
			RenderData.SetWeapon(pkt.GetShort());

			RenderData.SetSitting((SitState)pkt.GetChar());
			RenderData.SetHidden(pkt.GetChar() != 0);
		}

		/// <summary>
		/// Used to apply changes from Welcome packet to existing character.
		/// </summary>
		/// <param name="newGuy">Changes to MainPlayer.ActiveCharacter, contained in a Character object</param>
		public void ApplyData(Character newGuy)
		{
			ID = newGuy.ID;
			Name = newGuy.Name;
			PaddedGuildTag = newGuy.PaddedGuildTag;
			AdminLevel = newGuy.AdminLevel;
			Array.Copy(newGuy.PaperDoll, PaperDoll, (int)EquipLocation.PAPERDOLL_MAX);
			Stats = new CharStatData(newGuy.Stats);
			RenderData = newGuy.RenderData;
			RenderData.SetWalkFrame(0);
			Walking = false;
			CurrentMap = newGuy.CurrentMap;
			MapIsPk = newGuy.MapIsPk;
			X = newGuy.X;
			Y = newGuy.Y;
			GuildRankNum = newGuy.GuildRankNum;
		}

		/// <summary>
		/// Called from EOCharacterRenderer.Update (in case of MainPlayer pressing an arrow key) or Handlers.Walk (in case of another character walking)
		/// <para>The Character Renderer will automatically pick up that Walking == true and start a walk operation, limiting the character from walking again until it is complete</para>
		/// </summary>
		public void Walk(EODirection direction, byte destX, byte destY)
		{
			if (Walking)
				return;

			if (this == World.Instance.MainPlayer.ActiveCharacter)
				Handlers.Walk.PlayerWalk(direction, destX, destY, AdminLevel != AdminLevel.Player);
			else if (RenderData.facing != direction) //if a packet WALK_PLAYER was received: face them the right way first otherwise this will look weird
				RenderData.SetDirection(direction);

			destx = destX;
			desty = destY;
			Walking = true;
		}

		public void DoneWalking()
		{
			Walking = false;
			ViewAdjustX = 0;
			ViewAdjustY = 0;
			Walking = false; //this is the only place this should be set
			X = destx;
			Y = desty;
			RenderData.SetWalkFrame(0);
		}

		public void Face(EODirection direction)
		{
			//send packet to server: update client side if send was successful
			if(Handlers.Face.FacePlayer(direction))
				RenderData.SetDirection(direction); //updates the data in the character renderer as well
		}

		public void UpdateInventoryItem(short id, int characterAmount)
		{
			InventoryItem rec;
			if ((rec = Inventory.Find(item => item.id == id)).id == id)
			{
				InventoryItem newRec = new InventoryItem {amount = characterAmount, id = id};
				if (!Inventory.Remove(rec))
					throw new Exception("Unable to remove from inventory!");
				if (newRec.amount > 0)
				{
					Inventory.Add(newRec);
				}
				if (this == World.Instance.MainPlayer.ActiveCharacter) EOGame.Instance.Hud.UpdateInventory(rec);
			}
		}

		public void UpdateInventoryItem(short id, int characterAmount, byte characterWeight, byte characterMaxWeight)
		{
			InventoryItem rec;
			if ((rec = Inventory.Find(item => item.id == id)).id == id)
			{
				InventoryItem newRec = new InventoryItem {amount = characterAmount, id = id};
				if(!Inventory.Remove(rec))
					throw new Exception("Unable to remove from inventory!");
				if (newRec.amount > 0)
				{
					Inventory.Add(newRec);
				}
				if (this == World.Instance.MainPlayer.ActiveCharacter) EOGame.Instance.Hud.UpdateInventory(rec);
				Weight = characterWeight;
				MaxWeight = characterMaxWeight;
				if (this == World.Instance.MainPlayer.ActiveCharacter) EOGame.Instance.Hud.UpdateWeightLabel();
			}
		}
	}
}
