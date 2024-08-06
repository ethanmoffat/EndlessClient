using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.StatSkill
{
    /// <summary>
    /// Sent when spending skill points on a spell
    /// </summary>
    [AutoMappedType]
    public class StatskillAcceptHandler : InGameOnlyPacketHandler<StatSkillAcceptServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        public override PacketFamily Family => PacketFamily.StatSkill;

        public override PacketAction Action => PacketAction.Accept;

        public StatskillAcceptHandler(IPlayerInfoProvider playerInfoProvider,
                                      ICharacterRepository characterRepository,
                                      ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _characterInventoryRepository = characterInventoryRepository;
        }

        public override bool HandlePacket(StatSkillAcceptServerPacket packet)
        {
            if (packet.Spell.Id > 0)
            {
                _characterInventoryRepository.SpellInventory.RemoveWhere(x => x.ID == packet.Spell.Id);
                _characterInventoryRepository.SpellInventory.Add(new InventorySpell(packet.Spell.Id, packet.Spell.Level));
            }

            var stats = _characterRepository.MainCharacter.Stats.WithNewStat(CharacterStat.SkillPoints, packet.SkillPoints);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            return true;
        }
    }
}
