using EOLib.Data;

namespace EOLib.Net
{
	public enum AdminLevel
	{
		Player,
		// ReSharper disable UnusedMember.Global
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
		// ReSharper restore UnusedMember.Global
	}

	/// <summary>
	/// Represents data for a single character
	/// </summary>
	public struct CharacterData
	{
		private readonly string m_name;
		public string Name { get { return m_name; } }

		private readonly short m_id, m_map, m_x, m_y;
		public short ID { get { return m_id; } }
		public short Map { get { return m_map; } }
		public short X { get { return m_x; } }
		public short Y { get { return m_y; } }

		private readonly string m_guildTag;
		public string GuildTag { get { return m_guildTag; } }

		private readonly EODirection m_facing;
		public EODirection Direction { get { return m_facing; } }

		private readonly byte m_level, m_gender, m_hairstyle, m_haircolor, m_race;
		public byte Level { get { return m_level; } }
		public byte Gender { get { return m_gender; } }
		public byte HairStyle { get { return m_hairstyle; } }
		public byte HairColor { get { return m_haircolor; } }
		public byte Race { get { return m_race; } }

		private readonly short m_hp, m_maxhp, m_tp, m_maxtp;
		public short MaxHP { get { return m_maxhp; } }
		public short HP { get { return m_hp; } }
		public short MaxTP { get { return m_maxtp; } }
		public short TP { get { return m_tp; } }

		private readonly short m_boots, m_armor, m_hat, m_shield, m_weapon;
		public short Boots { get { return m_boots; } }
		public short Armor { get { return m_armor; } }
		public short Hat { get { return m_hat; } }
		public short Shield { get { return m_shield; } }
		public short Weapon { get { return m_weapon; } }

		private readonly SitState m_sit;
		private readonly bool m_hidden;
		public SitState Sitting { get { return m_sit; } }
		public bool Hidden { get { return m_hidden; } }

		internal CharacterData(Packet pkt)
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
}
