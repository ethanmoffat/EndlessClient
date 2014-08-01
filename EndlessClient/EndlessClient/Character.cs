using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EndlessClient
{
	//Represents the data for a character
	public class CharacterData
	{
		public string name;
		public int id;
		public byte level, gender, hairstyle, haircolor, race, admin;
		public short boots, armor, hat, shield, weapon;

		public CharacterData() { name = ""; }

		public void SetHairColor(byte color) { haircolor = color; }
		public void SetHairStyle(byte style) { hairstyle = style; }
		public void SetRace(byte newrace) { race = newrace; }
		public void SetGender(byte newgender) { gender = newgender; }

		public void SetShield(short newshield) { shield = newshield; }
		public void SetArmor(short newarmor) { armor = newarmor; }
		public void SetWeapon(short newweap) { weapon = newweap; }
		public void SetHat(short newhat) { hat = newhat; }
		public void SetBoots(short newboots) { boots = newboots; }
	}

	class Character
	{

	}
}
