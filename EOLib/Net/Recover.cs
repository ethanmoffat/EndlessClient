// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;

namespace EOLib.Net
{
	public struct DisplayStats
	{
		private readonly bool m_statsData;
		private readonly short m_class;

		private readonly byte m_maxWeight;
		private readonly short m_str, m_int, m_wis, m_agi, m_con, m_cha;
		private readonly short m_hp, m_tp, m_sp;
		private readonly short m_mindam, m_maxdam;
		private readonly short m_accuracy, m_evade, m_armor;

		//m_class is either a 'Class' id (for Recover_List) or the number
		//	of statpoints remaining (for StatSkill_Player). Check IsStatsData
		//	before using these objects.
		public byte Class { get { return (byte)m_class; } }
		public short StatPoints { get { return m_class; } }
		public bool IsStatsData { get { return m_statsData; } }

		public byte MaxWeight { get { return m_maxWeight; } }

		public short Str { get { return m_str; } }
		public short Int { get { return m_int; } }
		public short Wis { get { return m_wis; } }
		public short Agi { get { return m_agi; } }
		public short Con { get { return m_con; } }
		public short Cha { get { return m_cha; } }

		public short MaxHP { get { return m_hp; } }
		public short MaxTP { get { return m_tp; } }
		public short MaxSP { get { return m_sp; } }

		public short MinDam { get { return m_mindam; } }
		public short MaxDam { get { return m_maxdam; } }

		public short Accuracy { get { return m_accuracy; } }
		public short Evade { get { return m_evade; } }
		public short Armor { get { return m_armor; } }

		internal DisplayStats(Packet pkt, bool isStatsData)
		{
			m_statsData = isStatsData;
			m_class = pkt.GetShort();
			m_str = pkt.GetShort();
			m_int = pkt.GetShort();
			m_wis = pkt.GetShort();
			m_agi = pkt.GetShort();
			m_con = pkt.GetShort();
			m_cha = pkt.GetShort();
			m_hp = pkt.GetShort();
			m_tp = pkt.GetShort();
			m_sp = pkt.GetShort();
			m_maxWeight = (byte)pkt.GetShort();
			m_mindam = pkt.GetShort();
			m_maxdam = pkt.GetShort();
			m_accuracy = pkt.GetShort();
			m_evade = pkt.GetShort();
			m_armor = pkt.GetShort();
		}
	}

	public delegate void PlayerRecoverEvent(short HP, short TP);
	public delegate void RecoverReplyEvent(int exp, short karma, byte level, short statpoints = 0, short skillpoints = 0);
	public delegate void PlayerHealEvent(short playerID, int healAmount, byte percentHealth);

	partial class PacketAPI
	{
		public event PlayerRecoverEvent OnPlayerRecover;
		public event RecoverReplyEvent OnRecoverReply;
		public event PlayerHealEvent OnPlayerHeal;
		public event Action<DisplayStats> OnStatsList;

		private void _createRecoverMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Recover, PacketAction.Player), _handleRecoverPlayer, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Recover, PacketAction.Reply), _handleRecoverReply, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Recover, PacketAction.List), _handleRecoverList, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.Recover, PacketAction.Agree), _handleRecoverAgree, true);
		}

		private void _handleRecoverPlayer(Packet pkt)
		{
			if (OnPlayerRecover != null)
				OnPlayerRecover(pkt.GetShort(), pkt.GetShort()); //HP - TP
		}

		private void _handleRecoverReply(Packet pkt)
		{
			if (OnRecoverReply == null) return;

			int exp = pkt.GetInt();
			short karma = pkt.GetShort();
			byte level = pkt.GetChar();

			if (pkt.ReadPos == pkt.Length)
				OnRecoverReply(exp, karma, level);
			else
			{
				short statpoints = pkt.GetShort();
				short skillpoints = pkt.GetShort();

				OnRecoverReply(exp, karma, level, statpoints, skillpoints);
			}
		}

		private void _handleRecoverList(Packet pkt)
		{
			//almost identical to STATSKILL_PLAYER packet
			if (OnStatsList != null)
				OnStatsList(new DisplayStats(pkt, false));
		}

		private void _handleRecoverAgree(Packet pkt)
		{
			//when a heal item is used by another player
			if (OnPlayerHeal != null)
				OnPlayerHeal(pkt.GetShort(), pkt.GetInt(), pkt.GetChar()); //player id - hp gain - percent heal
		}
	}
}
