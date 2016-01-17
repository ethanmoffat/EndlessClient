// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Net
{
	public enum AvatarSlot : byte
	{
		Clothes = 1,
		Hair = 2,
		HairColor = 3
	};

	public struct AvatarData
	{
		private readonly short boots, armor, hat, shield, weapon;
		private readonly bool sound;
		private readonly byte hairstyle, haircolor;
		private readonly AvatarSlot slot;
		private readonly short pid;

		public short ID { get { return pid; } }
		public AvatarSlot Slot { get { return slot; } }

		public bool Sound { get { return sound; } }

		public short Boots { get { return boots; } }
		public short Armor { get { return armor; } }
		public short Hat { get { return hat; } }
		public short Shield { get { return shield; } }
		public short Weapon { get { return weapon; } }

		public byte HairStyle { get { return hairstyle; } }
		public byte HairColor { get { return haircolor; } }

		internal AvatarData(short id, AvatarSlot slot, bool sound, short boots, short armor, short hat, short weapon, short shield)
		{
			pid = id;
			this.slot = slot;

			this.sound = sound;
			this.boots = boots;
			this.armor = armor;
			this.hat = hat;
			this.shield = shield;
			this.weapon = weapon;

			hairstyle = haircolor = 0;
		}

		internal AvatarData(short id, AvatarSlot slot, byte hairStyle, byte hairColor)
		{
			pid = id;
			this.slot = slot;

			hairstyle = hairStyle;
			haircolor = hairColor;

			sound = false;
			boots = armor = hat = shield = weapon = 0;
		}
	}

	partial class PacketAPI
	{
		public event Action<short, WarpAnimation> OnPlayerAvatarRemove;
		public event Action<AvatarData> OnPlayerAvatarChange;

		private void _createAvatarMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Avatar, PacketAction.Remove), _handleAvatarRemove, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Avatar, PacketAction.Agree), _handleAvatarAgree, true);
		}

		// Remove a player from view (sent by server when someone is out of range)
		private void _handleAvatarRemove(Packet pkt)
		{
			if (OnPlayerAvatarRemove == null) return;

			short id = pkt.GetShort();
			WarpAnimation anim = (WarpAnimation)(pkt.Length > pkt.ReadPos ? pkt.GetChar() : 0);
			OnPlayerAvatarRemove(id, anim);
		}

		// Player changes appearance (clothes, hair, etc)
		private void _handleAvatarAgree(Packet pkt)
		{
			short playerID = pkt.GetShort();
			AvatarSlot slot = (AvatarSlot)pkt.GetChar();
			switch (slot)
			{
				case AvatarSlot.Clothes:
					{
						AvatarData newRenderData = new AvatarData(
							playerID,
							slot,
							pkt.GetChar() == 0,  //sound
							pkt.GetShort(),      //boots
							pkt.GetShort(),      //armor
							pkt.GetShort(),      //hat
							pkt.GetShort(),      //weapon
							pkt.GetShort()       //shield
						);
						if (OnPlayerAvatarChange != null)
							OnPlayerAvatarChange(newRenderData);
					}
					break;
				case AvatarSlot.Hair:
					{
						if (pkt.GetChar() != 0) return; //subloc -- not sure what this does
						AvatarData data = new AvatarData(playerID, slot, pkt.GetChar(), pkt.GetChar());
						if (OnPlayerAvatarChange != null)
							OnPlayerAvatarChange(data);
					}
					break;
				case AvatarSlot.HairColor:
					{
						if (pkt.GetChar() != 0) return; //subloc -- not sure what this does
						AvatarData data = new AvatarData(playerID, slot, 0, pkt.GetChar());
						if (OnPlayerAvatarChange != null)
							OnPlayerAvatarChange(data);
					}
					break;
			}
		}
	}
}
