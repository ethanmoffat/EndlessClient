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
    /// Sent when forgetting a skill
    /// </summary>
    [AutoMappedType]
    public class StatskillRemoveHandler : InGameOnlyPacketHandler<StatSkillRemoveServerPacket>
    {
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.StatSkill;

        public override PacketAction Action => PacketAction.Remove;

        public StatskillRemoveHandler(IPlayerInfoProvider playerInfoProvider,
                                      ICharacterInventoryRepository characterInventoryRepository,
                                      IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _characterInventoryRepository = characterInventoryRepository;
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(StatSkillRemoveServerPacket packet)
        {
            _characterInventoryRepository.SpellInventory.RemoveWhere(x => x.ID == packet.SpellId);

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifySkillForget();

            return true;
        }
    }
}