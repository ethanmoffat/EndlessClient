using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Recover
{
    /// <summary>
    /// Sent when the main character's stats are updated by a quest (or stat change command)
    /// </summary>
    [AutoMappedType]
    public class RecoverListHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;

        public override PacketFamily Family => PacketFamily.Recover;
        public override PacketAction Action => PacketAction.List;

        public RecoverListHandler(IPlayerInfoProvider playerInfoProvider,
                                  ICharacterRepository characterRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            //note: nearly identical code exists in StatTrainingHandler.HandlePacket
            //todo: consolidate
            var @class = packet.ReadShort();
            var str = packet.ReadShort();
            var intl = packet.ReadShort();
            var wis = packet.ReadShort();
            var agi = packet.ReadShort();
            var con = packet.ReadShort();
            var cha = packet.ReadShort();
            var hp = packet.ReadShort();
            var tp = packet.ReadShort();
            var sp = packet.ReadShort();
            var maxWeight = packet.ReadShort();
            var minDam = packet.ReadShort();
            var maxDam = packet.ReadShort();
            var accuracy = packet.ReadShort();
            var evade = packet.ReadShort();
            var armor = packet.ReadShort();

            var stats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.Strength, str)
                .WithNewStat(CharacterStat.Intelligence, intl)
                .WithNewStat(CharacterStat.Wisdom, wis)
                .WithNewStat(CharacterStat.Agility, agi)
                .WithNewStat(CharacterStat.Constituion, con)
                .WithNewStat(CharacterStat.Charisma, cha)
                .WithNewStat(CharacterStat.MaxHP, hp)
                .WithNewStat(CharacterStat.MaxTP, tp)
                .WithNewStat(CharacterStat.MaxSP, sp)
                .WithNewStat(CharacterStat.MaxWeight, maxWeight)
                .WithNewStat(CharacterStat.MinDam, minDam)
                .WithNewStat(CharacterStat.MaxDam, maxDam)
                .WithNewStat(CharacterStat.Accuracy, accuracy)
                .WithNewStat(CharacterStat.Evade, evade)
                .WithNewStat(CharacterStat.Armor, armor);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter
                .WithClassID((byte)@class)
                .WithStats(stats);

            return true;
        }
    }
}
