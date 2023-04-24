using System;
using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

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

            var packet = new PacketBuilder(PacketFamily.StatSkill, PacketAction.Add)
                .AddChar((int) TrainType.Stat)
                .AddShort(GetStatIndex(whichStat))
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void LevelUpSkill(int spellId)
        {
            var packet = new PacketBuilder(PacketFamily.StatSkill, PacketAction.Add)
                .AddChar((int)TrainType.Skill)
                .AddShort(spellId)
                .Build();

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
                case CharacterStat.Constituion:
                case CharacterStat.Charisma: return false;
                default: return true;
            }
        }

        private static int GetStatIndex(CharacterStat whichStat)
        {
            switch (whichStat)
            {
                case CharacterStat.Strength: return 1;
                case CharacterStat.Intelligence: return 2;
                case CharacterStat.Wisdom: return 3;
                case CharacterStat.Agility: return 4;
                case CharacterStat.Constituion: return 5;
                case CharacterStat.Charisma: return 6;
            }

            throw new ArgumentOutOfRangeException(nameof(whichStat));
        }
    }

    public interface ITrainingActions
    {
        void LevelUpStat(CharacterStat whichStat);

        void LevelUpSkill(int spellId);
    }
}
