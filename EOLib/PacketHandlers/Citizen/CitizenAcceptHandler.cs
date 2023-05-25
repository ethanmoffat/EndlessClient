using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Citizen
{
    /// <summary>
    /// Sent when the player has accepted the cost of sleeping at an inn
    /// </summary>
    [AutoMappedType]
    public class CitizenAcceptHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly ICharacterRepository _characterRepository;

        public override PacketFamily Family => PacketFamily.Citizen;

        public override PacketAction Action => PacketAction.Accept;

        public CitizenAcceptHandler(IPlayerInfoProvider playerInfoProvider,
                                    ICharacterInventoryRepository characterInventoryRepository,
                                    ICurrentMapStateRepository currentMapStateRepository,
                                    ICharacterRepository characterRepository)
            : base(playerInfoProvider)
        {
            _characterInventoryRepository = characterInventoryRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _characterRepository = characterRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var goldRemaining = packet.ReadInt();
            _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == 1);
            _characterInventoryRepository.ItemInventory.Add(new InventoryItem(1, goldRemaining));

            var stats = _characterRepository.MainCharacter.Stats;
            stats = stats.WithNewStat(CharacterStat.HP, stats[CharacterStat.MaxHP])
                .WithNewStat(CharacterStat.TP, stats[CharacterStat.MaxTP]);
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            _currentMapStateRepository.IsSleepWarp = true;

            return true;
        }
    }
}
