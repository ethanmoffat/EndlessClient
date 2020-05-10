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

        public short ID => pid;
        public AvatarSlot Slot => slot;

        public bool Sound => sound;

        public short Boots => boots;
        public short Armor => armor;
        public short Hat => hat;
        public short Shield => shield;
        public short Weapon => weapon;

        public byte HairStyle => hairstyle;
        public byte HairColor => haircolor;

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
