using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Interact;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers.StatSkill
{
    /// <summary>
    /// Sent when learning a skill, either via $learn command or from skillmaster
    /// </summary>
    [AutoMappedType]
    public class StatskillTakeHandler : InGameOnlyPacketHandler<StatSkillTakeServerPacket>
    {
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IEnumerable<INPCInteractionNotifier> _npcInteractionNotifiers;

        public override PacketFamily Family => PacketFamily.StatSkill;

        public override PacketAction Action => PacketAction.Take;

        public StatskillTakeHandler(IPlayerInfoProvider playerInfoProvider,
                                    ICharacterInventoryRepository characterInventoryRepository,
                                    IEnumerable<INPCInteractionNotifier> npcInteractionNotifiers)
            : base(playerInfoProvider)
        {
            _characterInventoryRepository = characterInventoryRepository;
            _npcInteractionNotifiers = npcInteractionNotifiers;
        }

        public override bool HandlePacket(StatSkillTakeServerPacket packet)
        {
            if (!_characterInventoryRepository.SpellInventory.Any(x => x.ID == packet.SpellId))
            {
                _characterInventoryRepository.SpellInventory.Add(new InventorySpell(packet.SpellId, 0));
            }

            _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
            _characterInventoryRepository.ItemInventory.Add(new InventoryItem(1, packet.GoldAmount));

            foreach (var notifier in _npcInteractionNotifiers)
                notifier.NotifySkillLearnSuccess(packet.SpellId, packet.GoldAmount);

            return true;
        }
    }
}
