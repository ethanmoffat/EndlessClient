using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Interact;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.StatSkill
{
    /// <summary>
    /// Sent when resetting a character's stats/skills
    /// </summary>
    [AutoMappedType]
    public class StatskillJunkHandler : InGameOnlyPacketHandler<StatSkillJunkServerPacket>
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

        public override bool HandlePacket(StatSkillJunkServerPacket packet)
        {
            var stats = _characterRepository.MainCharacter.Stats.Apply(CharacterStats.FromStatReset(packet.Stats));
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            _characterInventoryRepository.SpellInventory.Clear();

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifyStatReset();

            return true;
        }
    }
}