using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Interact;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.StatSkill
{
    /// <summary>
    /// Sent when resetting a character's stats/skills
    /// </summary>
    [AutoMappedType]
    public class StatskillJunkHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly ICharacterRepository _characterRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.StatSkill;

        public override PacketAction Action => PacketAction.Junk;

        public StatskillJunkHandler(IPlayerInfoProvider playerInfoProvider,
                                    ICharacterInventoryRepository characterInventoryRepository,
                                    ICharacterRepository characterRepository,
                                    IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _characterInventoryRepository = characterInventoryRepository;
            _characterRepository = characterRepository;
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var stats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.StatPoints, packet.ReadShort())
                .WithNewStat(CharacterStat.SkillPoints, packet.ReadShort())
                .WithNewStat(CharacterStat.HP, packet.ReadShort())
                .WithNewStat(CharacterStat.MaxHP, packet.ReadShort())
                .WithNewStat(CharacterStat.TP, packet.ReadShort())
                .WithNewStat(CharacterStat.MaxTP, packet.ReadShort())
                .WithNewStat(CharacterStat.MaxSP, packet.ReadShort())
                .WithNewStat(CharacterStat.Strength, packet.ReadShort())
                .WithNewStat(CharacterStat.Intelligence, packet.ReadShort())
                .WithNewStat(CharacterStat.Wisdom, packet.ReadShort())
                .WithNewStat(CharacterStat.Agility, packet.ReadShort())
                .WithNewStat(CharacterStat.Constituion, packet.ReadShort())
                .WithNewStat(CharacterStat.Charisma, packet.ReadShort())
                .WithNewStat(CharacterStat.MinDam, packet.ReadShort())
                .WithNewStat(CharacterStat.MaxDam, packet.ReadShort())
                .WithNewStat(CharacterStat.Accuracy, packet.ReadShort())
                .WithNewStat(CharacterStat.Evade, packet.ReadShort())
                .WithNewStat(CharacterStat.Armor, packet.ReadShort());
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            _characterInventoryRepository.SpellInventory.Clear();

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyStatReset();

            return true;
        }
    }
}
