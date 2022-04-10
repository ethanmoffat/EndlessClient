using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional.Collections;

namespace EOLib.PacketHandlers.Locker
{
    /// <summary>
    /// Handles LOCKER_GET from server for taking an item from locker
    /// </summary>
    [AutoMappedType]
    public class LockerGetHandler : InGameOnlyPacketHandler
    {
        private readonly ILockerDataRepository _lockerDataRepository;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        public override PacketFamily Family => PacketFamily.Locker;

        public override PacketAction Action => PacketAction.Get;

        public LockerGetHandler(IPlayerInfoProvider playerInfoProvider,
                                ILockerDataRepository lockerDataRepository,
                                ICharacterRepository characterRepository,
                                ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider)
        {
            _lockerDataRepository = lockerDataRepository;
            _characterRepository = characterRepository;
            _characterInventoryRepository = characterInventoryRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var itemId = packet.ReadShort();
            var amount = Action == PacketAction.Get ? packet.ReadThree() : packet.ReadInt();
            var weight = packet.ReadChar();
            var maxWeight = packet.ReadChar();

            _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == itemId)
                .Match(
                    some: existing =>
                    {
                        _characterInventoryRepository.ItemInventory.Remove(existing);
                        if (amount > 0 || itemId == 1)
                        {
                            _characterInventoryRepository.ItemInventory.Add(existing.WithAmount(Action == PacketAction.Get ? existing.Amount + amount : amount));
                        }
                    },
                    none: () =>
                    {
                        if (amount > 0)
                            _characterInventoryRepository.ItemInventory.Add(new InventoryItem(itemId, amount));
                    });

            var stats = _characterRepository.MainCharacter.Stats
                .WithNewStat(CharacterStat.Weight, weight)
                .WithNewStat(CharacterStat.MaxWeight, maxWeight);

            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);

            _lockerDataRepository.Items.Clear();
            while (packet.ReadPosition < packet.Length)
                _lockerDataRepository.Items.Add(new InventoryItem(packet.ReadShort(), packet.ReadThree()));

            return true;
        }
    }

    /// <summary>
    /// Handles LOCKER_REPLY from server for adding an item to locker
    /// </summary>
    [AutoMappedType]
    public class LockerReplyHandler : LockerGetHandler
    {
        public override PacketAction Action => PacketAction.Reply;

        public LockerReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                  ILockerDataRepository lockerDataRepository,
                                  ICharacterRepository characterRepository,
                                  ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider, lockerDataRepository, characterRepository, characterInventoryRepository)
        {
        }
    }
}
