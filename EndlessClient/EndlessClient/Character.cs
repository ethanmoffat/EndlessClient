using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EOLib;

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
		public string name;
		public int id;
		public byte level, gender, hairstyle, haircolor, race, admin;
		public short boots, armor, hat, shield, weapon;
		
		public EODirection facing;
		public SitState sitting;
		public bool hidden;

		public CharRenderData() { name = ""; }

		public void SetHairColor(byte color) { haircolor = color; }
		public void SetHairStyle(byte style) { hairstyle = style; }
		public void SetRace(byte newrace) { race = newrace; }
		public void SetGender(byte newgender) { gender = newgender; }

		public void SetDirection(EODirection direction) { facing = direction; }
		public void SetSitting(SitState sits) { sitting = sits; }
		public void SetHidden(bool hiding) { hidden = hiding; }

		public void SetShield(short newshield) { shield = newshield; }
		public void SetArmor(short newarmor) { armor = newarmor; }
		public void SetWeapon(short newweap) { weapon = newweap; }
		public void SetHat(short newhat) { hat = newhat; }
		public void SetBoots(short newboots) { boots = newboots; }
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

	public class Character
	{
		public int ID { get; private set; }

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
			if (data == null)
				RenderData = new CharRenderData();

			Inventory = new List<InventoryItem>();
			Spells = new List<CharacterSpell>();
			PaperDoll = new short[(int)EquipLocation.PAPERDOLL_MAX];
		}
	}
}
