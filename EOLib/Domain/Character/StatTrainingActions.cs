// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Character
{
    //todo: maybe this should go into its own namespace? Domain.Character is pretty monolithic
    [AutoMappedType]
    public class StatTrainingActions : IStatTrainingActions
    {
        private readonly IPacketSendService _packetSendService;

        public StatTrainingActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void LevelUpStat(CharacterStat whichStat)
        {
            if (InvalidStat(whichStat))
                return;

            var packet = new PacketBuilder(PacketFamily.StatSkill, PacketAction.Add)
                .AddChar((byte) TrainType.Stat)
                .AddShort((byte) GetStatIndex(whichStat))
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

        private static short GetStatIndex(CharacterStat whichStat)
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

    public interface IStatTrainingActions
    {
        void LevelUpStat(CharacterStat whichStat);
    }
}
