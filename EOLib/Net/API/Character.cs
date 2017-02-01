// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Character;

namespace EOLib.Net.API
{
    /// <summary>
    /// Represents data for a single character
    /// </summary>
    public struct CharacterData
    {
        private readonly string m_name;
        public string Name => m_name;

        private readonly short m_id, m_map, m_x, m_y;
        public short ID => m_id;
        public short Map => m_map;
        public short X => m_x;
        public short Y => m_y;

        private readonly string m_guildTag;
        public string GuildTag => m_guildTag;

        private readonly EODirection m_facing;
        public EODirection Direction => m_facing;

        private readonly byte m_level, m_gender, m_hairstyle, m_haircolor, m_race;
        public byte Level => m_level;
        public byte Gender => m_gender;
        public byte HairStyle => m_hairstyle;
        public byte HairColor => m_haircolor;
        public byte Race => m_race;

        private readonly short m_hp, m_maxhp, m_tp, m_maxtp;
        public short MaxHP => m_maxhp;
        public short HP => m_hp;
        public short MaxTP => m_maxtp;
        public short TP => m_tp;

        private readonly short m_boots, m_armor, m_hat, m_shield, m_weapon;
        public short Boots => m_boots;
        public short Armor => m_armor;
        public short Hat => m_hat;
        public short Shield => m_shield;
        public short Weapon => m_weapon;

        private readonly SitState m_sit;
        private readonly bool m_hidden;
        public SitState Sitting => m_sit;
        public bool Hidden => m_hidden;

        internal CharacterData(OldPacket pkt)
        {
            m_name = pkt.GetBreakString();
            if (m_name.Length > 1)
                m_name = char.ToUpper(m_name[0]) + m_name.Substring(1);

            m_id = pkt.GetShort();
            m_map = pkt.GetShort();
            m_x = pkt.GetShort();
            m_y = pkt.GetShort();

            m_facing = (EODirection)pkt.GetChar();
            pkt.GetChar(); //value is always 6? unknown
            m_guildTag = pkt.GetFixedString(3);

            m_level = pkt.GetChar();
            m_gender = pkt.GetChar();
            m_hairstyle = pkt.GetChar();
            m_haircolor = pkt.GetChar();
            m_race = pkt.GetChar();

            m_maxhp = pkt.GetShort();
            m_hp = pkt.GetShort();
            m_maxtp = pkt.GetShort();
            m_tp = pkt.GetShort();

            m_boots = pkt.GetShort();
            pkt.Skip(3 * sizeof(short)); //other paperdoll data is 0'd out
            m_armor = pkt.GetShort();
            pkt.Skip(sizeof(short));
            m_hat = pkt.GetShort();
            m_shield = pkt.GetShort();
            m_weapon = pkt.GetShort();

            m_sit = (SitState)pkt.GetChar();
            m_hidden = pkt.GetChar() != 0;
        }
    }

    /// <summary>
    /// Represents render data for a single character
    /// </summary>
    public struct CharacterLoginData
    {
        private readonly string name;
        private readonly int id;
        private readonly byte level, gender, hairstyle, haircolor, race;
        private readonly AdminLevel admin;
        private readonly short boots, armor, hat, shield, weapon;

        public string Name => name;
        public int ID => id;
        public byte Level => level;
        public byte Gender => gender;
        public byte HairStyle => hairstyle;
        public byte HairColor => haircolor;
        public byte Race => race;
        public AdminLevel AdminLevel => admin;
        public short Boots => boots;
        public short Armor => armor;
        public short Hat => hat;
        public short Shield => shield;
        public short Weapon => weapon;

        internal CharacterLoginData(OldPacket pkt)
        {
            name = pkt.GetBreakString();
            id = pkt.GetInt();
            level = pkt.GetChar();
            gender = pkt.GetChar();
            hairstyle = pkt.GetChar();
            haircolor = pkt.GetChar();
            race = pkt.GetChar();
            admin = (AdminLevel)pkt.GetChar();
            boots = pkt.GetShort();
            armor = pkt.GetShort();
            hat = pkt.GetShort();
            shield = pkt.GetShort();
            weapon = pkt.GetShort();
        }
    }
}
