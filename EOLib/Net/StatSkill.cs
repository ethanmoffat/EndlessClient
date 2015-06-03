using System;
using System.Collections.Generic;

namespace EOLib.Net
{
	internal enum TrainType
	{
		Stat = 1,
		Skill = 2
	}

	public enum SkillMasterReply
	{
// ReSharper disable once UnusedMember.Global
		ErrorRemoveItems = 1,
		ErrorWrongClass = 2
	}

	public struct Skill
	{
		private readonly short m_id;
		private readonly byte m_levelReq;
		private readonly  byte m_classReq;
		private readonly int m_goldCost;
		private readonly short[] m_skillReq; //ids of other skills that are required to know before learning this skill
		private readonly short m_strReq;
		private readonly short m_intReq;
		private readonly short m_wisReq;
		private readonly short m_agiReq;
		private readonly short m_conReq;
		private readonly short m_chaReq;

		public short ID { get { return m_id; } }
		public byte LevelReq { get { return m_levelReq; } }
		public byte ClassReq { get { return m_classReq; } }
		public int GoldReq { get { return m_goldCost; } }
		public short[] SkillReq { get { return m_skillReq; } }
		public short StrReq { get { return m_strReq; } }
		public short IntReq { get { return m_intReq; } }
		public short WisReq { get { return m_wisReq; } }
		public short AgiReq { get { return m_agiReq; } }
		public short ConReq { get { return m_conReq; } }
		public short ChaReq { get { return m_chaReq; } }

		internal Skill(Packet pkt)
		{
			m_id = pkt.GetShort();
			m_levelReq = pkt.GetChar();
			m_classReq = pkt.GetChar();
			m_goldCost = pkt.GetInt();
			m_skillReq = new[]
			{
				pkt.GetShort(),
				pkt.GetShort(),
				pkt.GetShort(),
				pkt.GetShort()
			};
			m_strReq = pkt.GetShort();
			m_intReq = pkt.GetShort();
			m_wisReq = pkt.GetShort();
			m_agiReq = pkt.GetShort();
			m_conReq = pkt.GetShort();
			m_chaReq = pkt.GetShort();
		}
	}

	public struct SkillmasterData
	{
		private readonly short m_id;
		private readonly string m_title;

		private readonly List<Skill> m_skills;

		public short ID { get { return m_id; } }
		public string Title { get { return m_title; } }
		public IList<Skill> Skills { get { return m_skills.AsReadOnly(); } }

		internal SkillmasterData(Packet pkt)
		{
			m_id = pkt.GetShort();
			m_title = pkt.GetBreakString();
			m_skills = new List<Skill>();
			while(pkt.ReadPos < pkt.Length)
				m_skills.Add(new Skill(pkt));
		}
	}

	public delegate void SpellLearnErrorEvent(SkillMasterReply reply, short classID);
	public delegate void SpellLearnSuccessEvent(short spellID, int goldRemaining);
	public delegate void SpellForgetEvent(short spellID);
	public delegate void SpellTrainEvent(short skillPtsRemaining, short spellID, short spellLevel);

	partial class PacketAPI
	{
		public event Action<SkillmasterData> OnSkillmasterOpen;
		public event SpellLearnErrorEvent OnSpellLearnError;
		public event SpellLearnSuccessEvent OnSpellLearnSuccess;
		public event SpellForgetEvent OnSpellForget;
		public event SpellTrainEvent OnSpellTrain;

		private void _createStatSkillMembers()
		{
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Open), _handleStatSkillOpen, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Reply), _handleStatSkillReply, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Take), _handleStatSkillTake, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Remove), _handleStatSkillRemove, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Player), _handleStatSkillPlayer, true);
			m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Accept), _handleStatSkillAccept, true);
		}

		public bool RequestSkillmaster(short skillmasterIndex)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.StatSkill, PacketAction.Open);
			pkt.AddShort(skillmasterIndex);

			return m_client.SendPacket(pkt);
		}

		public bool LearnSpell(short spellID)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.StatSkill, PacketAction.Take);
			pkt.AddInt(1234); //shop ID, ignored by eoserv - eomain may require this to be correct
			pkt.AddShort(spellID);

			return m_client.SendPacket(pkt);
		}

		public bool ForgetSpell(short spellID)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.StatSkill, PacketAction.Remove);
			pkt.AddInt(1234); //shop ID, ignored by eoserv - eomain may require this to be correct
			pkt.AddShort(spellID);

			return m_client.SendPacket(pkt);
		}

		public bool LevelUpStat(short statID)
		{
			return _trainStatShared(statID, TrainType.Stat);
		}

		public bool LevelUpSpell(short spellID)
		{
			return _trainStatShared(spellID, TrainType.Skill);
		}

		public bool ResetCharacterStatSkill()
		{
			Packet pkt = new Packet(PacketFamily.StatSkill, PacketAction.Junk);
			pkt.AddInt(1234); //shop ID, ignored by eoserv - eomain may require this to be correct
			return !m_client.ConnectedAndInitialized || !Initialized || !m_client.SendPacket(pkt);
		}

		private bool _trainStatShared(short id, TrainType type)
		{
			if (!m_client.ConnectedAndInitialized || !Initialized)
				return false;

			Packet pkt = new Packet(PacketFamily.StatSkill, PacketAction.Add);
			pkt.AddChar((byte)type);
			pkt.AddShort(id);

			return m_client.SendPacket(pkt);
		}

		//handlers

		private void _handleStatSkillOpen(Packet pkt)
		{
			if (OnSkillmasterOpen != null)
				OnSkillmasterOpen(new SkillmasterData(pkt));
		}

		//error learning a skill
		private void _handleStatSkillReply(Packet pkt)
		{
			//short - should always be SKILLMASTER_REMOVE_ITEMS (1) or SKILLMASTER_WRONG_CLASS (2)
			//short - character class
			if (OnSpellLearnError != null)
				OnSpellLearnError((SkillMasterReply)pkt.GetShort(), pkt.GetShort());
		}

		//success learning a skill
		private void _handleStatSkillTake(Packet pkt)
		{
			//short - spell id
			//int - character gold remaining
			if (OnSpellLearnSuccess != null)
				OnSpellLearnSuccess(pkt.GetShort(), pkt.GetInt());
		}

		//forgetting a skill
		private void _handleStatSkillRemove(Packet pkt)
		{
			//short - spell id
			if (OnSpellForget != null)
				OnSpellForget(pkt.GetShort());
		}

		//stat point added
		private void _handleStatSkillPlayer(Packet pkt)
		{
			if (OnStatsList != null)
				OnStatsList(new DisplayStats(pkt, true));
		}

		//skill point added to spell
		private void _handleStatSkillAccept(Packet pkt)
		{
			//short - character skill pts remaining
			//short - stat ID (spell ID)
			//short - spell level
			if (OnSpellTrain != null)
				OnSpellTrain(pkt.GetShort(), pkt.GetShort(), pkt.GetShort());
		}
	}
}
