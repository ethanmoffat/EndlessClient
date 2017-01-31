// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Character;

namespace EOLib.Net.API
{
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
}
