using System;
using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
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

        public short ID => m_id;
        public byte LevelReq => m_levelReq;
        public byte ClassReq => m_classReq;
        public int GoldReq => m_goldCost;
        public short[] SkillReq => m_skillReq;
        public short StrReq => m_strReq;
        public short IntReq => m_intReq;
        public short WisReq => m_wisReq;
        public short AgiReq => m_agiReq;
        public short ConReq => m_conReq;
        public short ChaReq => m_chaReq;

        internal Skill(OldPacket pkt)
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

    public struct StatResetData
    {
        private readonly short m_statpts, m_skillpts, m_hp, m_maxhp, m_tp, m_maxtp, m_maxsp;
        private readonly short m_str, m_int, m_wis, m_agi, m_con, m_cha;
        private readonly short m_mindam, m_maxdam, m_acc, m_evade, m_armor;

        public short StatPoints => m_statpts;
        public short SkillPoints => m_skillpts;
        public short HP => m_hp;
        public short MaxHP => m_maxhp;
        public short TP => m_tp;
        public short MaxTP => m_maxtp;
        public short MaxSP => m_maxsp;

        public short Str => m_str;
        public short Int => m_int;
        public short Wis => m_wis;
        public short Agi => m_agi;
        public short Con => m_con;
        public short Cha => m_cha;

        public short MinDam => m_mindam;
        public short MaxDam => m_maxdam;
        public short Accuracy => m_acc;
        public short Evade => m_evade;
        public short Armor => m_armor;

        internal StatResetData(OldPacket pkt)
        {
            m_statpts = pkt.GetShort();
            m_skillpts = pkt.GetShort();
            m_hp = pkt.GetShort();
            m_maxhp = pkt.GetShort();
            m_tp = pkt.GetShort();
            m_maxtp = pkt.GetShort();
            m_maxsp = pkt.GetShort();

            m_str = pkt.GetShort();
            m_int = pkt.GetShort();
            m_wis = pkt.GetShort();
            m_agi = pkt.GetShort();
            m_con = pkt.GetShort();
            m_cha = pkt.GetShort();

            m_mindam = pkt.GetShort();
            m_maxdam = pkt.GetShort();
            m_acc = pkt.GetShort();
            m_evade = pkt.GetShort();
            m_armor = pkt.GetShort();
        }
    }

    public delegate void SpellLearnErrorEvent(SkillMasterReply reply, short classID);
    public delegate void SpellForgetEvent(short spellID);

    partial class PacketAPI
    {
        public event SpellLearnErrorEvent OnSpellLearnError;
        public event SpellForgetEvent OnSpellForget;
        public event Action<StatResetData> OnCharacterStatsReset;

        private void _createStatSkillMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Reply), _handleStatSkillReply, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Remove), _handleStatSkillRemove, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Junk), _handleStatSkillJunk, true);
        }

        public bool LearnSpell(short spellID)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.StatSkill, PacketAction.Take);
            pkt.AddInt(1234); //shop ID, ignored by eoserv - eomain may require this to be correct
            pkt.AddShort(spellID);

            return m_client.SendPacket(pkt);
        }

        public bool ForgetSpell(short spellID)
        {
            if (!m_client.ConnectedAndInitialized || !Initialized)
                return false;

            OldPacket pkt = new OldPacket(PacketFamily.StatSkill, PacketAction.Remove);
            pkt.AddInt(1234); //shop ID, ignored by eoserv - eomain may require this to be correct
            pkt.AddShort(spellID);

            return m_client.SendPacket(pkt);
        }

        public bool ResetCharacterStatSkill()
        {
            OldPacket pkt = new OldPacket(PacketFamily.StatSkill, PacketAction.Junk);
            pkt.AddInt(1234); //shop ID, ignored by eoserv - eomain may require this to be correct
            return !m_client.ConnectedAndInitialized || !Initialized || m_client.SendPacket(pkt);
        }

        //handlers

        //error learning a skill
        private void _handleStatSkillReply(OldPacket pkt)
        {
            //short - should always be SKILLMASTER_REMOVE_ITEMS (1) or SKILLMASTER_WRONG_CLASS (2)
            //short - character class
            if (OnSpellLearnError != null)
                OnSpellLearnError((SkillMasterReply)pkt.GetShort(), pkt.GetShort());
        }

        //forgetting a skill
        private void _handleStatSkillRemove(OldPacket pkt)
        {
            //short - spell id
            if (OnSpellForget != null)
                OnSpellForget(pkt.GetShort());
        }

        //reset character
        private void _handleStatSkillJunk(OldPacket pkt)
        {
            if (OnCharacterStatsReset != null)
                OnCharacterStatsReset(new StatResetData(pkt));
        }
    }
}
