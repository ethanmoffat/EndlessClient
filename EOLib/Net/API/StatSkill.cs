using System;
using System.Collections.Generic;
using EOLib.Domain.Character;
using EOLib.Net.Handlers;

namespace EOLib.Net.API
{
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

    public delegate void SpellForgetEvent(short spellID);

    partial class PacketAPI
    {
        public event SpellForgetEvent OnSpellForget;
        public event Action<StatResetData> OnCharacterStatsReset;

        private void _createStatSkillMembers()
        {
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Remove), _handleStatSkillRemove, true);
            m_client.AddPacketHandler(new FamilyActionPair(PacketFamily.StatSkill, PacketAction.Junk), _handleStatSkillJunk, true);
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
