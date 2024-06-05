using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using System;

namespace EOLib.Domain.Character
{
    [AutoMappedType]
    public class TrainingActions : ITrainingActions
    {
        private readonly IPacketSendService _packetSendService;

        public TrainingActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void LevelUpStat(CharacterStat whichStat)
        {
            if (InvalidStat(whichStat))
                return;

            var packet = new StatSkillAddClientPacket
            {
                ActionType = TrainType.Stat,
                ActionTypeData = new StatSkillAddClientPacket.ActionTypeDataStat
                {
                    StatId = (StatId)GetStatIndex(whichStat)
                }
            };
            _packetSendService.SendPacket(packet);
        }

        public void LevelUpSkill(int spellId)
        {
            var packet = new StatSkillAddClientPacket
            {
                ActionType = TrainType.Skill,
                ActionTypeData = new StatSkillAddClientPacket.ActionTypeDataSkill
                {
                    SpellId = spellId
                }
            };
            _packetSendService.SendPacket(packet);
        }

        private static bool InvalidStat(CharacterStat whichStat)
        {
            switch (whichStat)
            {
                case CharacterStat.Strength:
                case CharacterStat.Intelligence:
                case CharacterStat.Wisdom:
                case CharacterStat.Agility:
                case CharacterStat.Constitution:
                case CharacterStat.Charisma: return false;
                default: return true;
            }
        }

        private static int GetStatIndex(CharacterStat whichStat)
        {
            return whichStat switch
            {
                CharacterStat.Strength => 1,
                CharacterStat.Intelligence => 2,
                CharacterStat.Wisdom => 3,
                CharacterStat.Agility => 4,
                CharacterStat.Constitution => 5,
                CharacterStat.Charisma => 6,
                _ => throw new ArgumentOutOfRangeException(nameof(whichStat)),
            };
        }
    }

    public interface ITrainingActions
    {
        void LevelUpStat(CharacterStat whichStat);

        void LevelUpSkill(int spellId);
    }
}
