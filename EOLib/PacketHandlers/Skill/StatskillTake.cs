using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Linq;

namespace EOLib.PacketHandlers.Skill
{
    /// <summary>
    /// Sent when learning a skill, either via $learn command or from skillmaster
    /// </summary>
    [AutoMappedType]
    public class StatskillTake : InGameOnlyPacketHandler
    {
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        public override PacketFamily Family => PacketFamily.StatSkill;

        public override PacketAction Action => PacketAction.Take;

        public StatskillTake(IPlayerInfoProvider playerInfoProvider,
                             ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider)
        {
            _characterInventoryRepository = characterInventoryRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var spellId = packet.ReadShort();
            var characterGold = packet.ReadInt();

            if (!_characterInventoryRepository.SpellInventory.Any(x => x.ID == spellId))
            {
                _characterInventoryRepository.SpellInventory.Add(new InventorySpell(spellId, 0));
            }

            _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
            _characterInventoryRepository.ItemInventory.Add(new InventoryItem(1, characterGold));

            return true;
        }
    }
}
