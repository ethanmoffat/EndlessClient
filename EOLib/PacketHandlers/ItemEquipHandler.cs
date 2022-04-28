using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.IO.Extensions;
using EOLib.IO.Repositories;
using EOLib.Net;
using Optional;
using Optional.Collections;
using System.Linq;

namespace EOLib.PacketHandlers
{
    public abstract class ItemEquipHandler : PlayerAvatarChangeHandler
    {
        private readonly IPaperdollRepository _paperdollRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;

        protected ItemEquipHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICurrentMapStateRepository currentMapStateRepository,
                                   ICharacterRepository characterRepository,
                                   IEIFFileProvider eifFileProvider,
                                   IPaperdollRepository paperdollRepository,
                                   ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository, eifFileProvider)
        {
            _paperdollRepository = paperdollRepository;
            _characterInventoryRepository = characterInventoryRepository;
        }

        protected bool HandlePaperdollPacket(IPacket packet, bool itemUnequipped)
        {
            var playerId = packet.PeekShort();

            if (!base.HandlePacket(packet))
                return false;

            var itemId = packet.ReadShort();
            var amount = itemUnequipped ? 1 : packet.ReadThree();
            var subLoc = packet.ReadChar();

            var maxhp = packet.ReadShort();
            var maxtp = packet.ReadShort();
            var disp_str = packet.ReadShort();
            var disp_int = packet.ReadShort();
            var disp_wis = packet.ReadShort();
            var disp_agi = packet.ReadShort();
            var disp_con = packet.ReadShort();
            var disp_cha = packet.ReadShort();
            var mindam = packet.ReadShort();
            var maxdam = packet.ReadShort();
            var accuracy = packet.ReadShort();
            var evade = packet.ReadShort();
            var armor = packet.ReadShort();

            if (_paperdollRepository.VisibleCharacterPaperdolls.ContainsKey(playerId))
            {
                var paperdollData = _paperdollRepository.VisibleCharacterPaperdolls[playerId];

                var itemRec = _eifFileProvider.EIFFile[itemId];
                var paperdollSlot = itemRec.GetEquipLocation() + subLoc;

                var paperdollEquipData = paperdollData.Paperdoll.ToDictionary(k => k.Key, v => v.Value);
                paperdollEquipData[paperdollSlot] = itemUnequipped ? (short)0 : itemId;

                _paperdollRepository.VisibleCharacterPaperdolls[playerId] = paperdollData.WithPaperdoll(paperdollEquipData);
            }

            var update = _characterRepository.MainCharacter.ID == playerId
                ? Option.Some(_characterRepository.MainCharacter)
                : _currentMapStateRepository.Characters.ContainsKey(playerId)
                    ? Option.Some(_currentMapStateRepository.Characters[playerId])
                    : Option.None<Character>();

            update.MatchSome(c =>
            {
                var stats = c.Stats
                    .WithNewStat(CharacterStat.MaxHP, maxhp)
                    .WithNewStat(CharacterStat.MaxTP, maxtp)
                    .WithNewStat(CharacterStat.Strength, disp_str)
                    .WithNewStat(CharacterStat.Intelligence, disp_int)
                    .WithNewStat(CharacterStat.Wisdom, disp_wis)
                    .WithNewStat(CharacterStat.Agility, disp_agi)
                    .WithNewStat(CharacterStat.Constituion, disp_con)
                    .WithNewStat(CharacterStat.Charisma, disp_cha)
                    .WithNewStat(CharacterStat.MinDam, mindam)
                    .WithNewStat(CharacterStat.MaxDam, maxdam)
                    .WithNewStat(CharacterStat.Accuracy, accuracy)
                    .WithNewStat(CharacterStat.Evade, evade)
                    .WithNewStat(CharacterStat.Armor, armor);

                if (c == _characterRepository.MainCharacter)
                {
                    _characterRepository.MainCharacter = c.WithStats(stats);

                    var updatedItem = _characterInventoryRepository.ItemInventory
                        .SingleOrNone(x => x.ItemID == itemId)
                        .Match(some: invItem => invItem.WithAmount(itemUnequipped ? invItem.Amount + amount : amount),
                               none: () => new InventoryItem(itemId, amount));

                    _characterInventoryRepository.ItemInventory.RemoveWhere(x => x.ItemID == itemId);

                    if (updatedItem.Amount > 0)
                        _characterInventoryRepository.ItemInventory.Add(updatedItem);
                }
                else
                {
                    _currentMapStateRepository.Characters[playerId] = c.WithStats(stats);
                }
            });

            update.MatchNone(() =>
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(playerId);
            });

            return true;
        }
    }

    [AutoMappedType]
    public class PaperdollAgreeHandler : ItemEquipHandler
    {
        public override PacketFamily Family => PacketFamily.PaperDoll;

        public override PacketAction Action => PacketAction.Agree;

        public PaperdollAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                      ICurrentMapStateRepository currentMapStateRepository,
                                      ICharacterRepository characterRepository,
                                      IEIFFileProvider eifFileProvider,
                                      IPaperdollRepository paperdollRepository,
                                      ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository, eifFileProvider, paperdollRepository, characterInventoryRepository)
        {
        }

        public override bool HandlePacket(IPacket packet)
        {
            return HandlePaperdollPacket(packet, itemUnequipped: false);
        }
    }

    [AutoMappedType]
    public class PaperdollRemoveHandler : ItemEquipHandler
    {
        public override PacketFamily Family => PacketFamily.PaperDoll;

        public override PacketAction Action => PacketAction.Remove;

        public PaperdollRemoveHandler(IPlayerInfoProvider playerInfoProvider,
                                      ICurrentMapStateRepository currentMapStateRepository,
                                      ICharacterRepository characterRepository,
                                      IEIFFileProvider eifFileProvider,
                                      IPaperdollRepository paperdollRepository,
                                      ICharacterInventoryRepository characterInventoryRepository)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository, eifFileProvider, paperdollRepository, characterInventoryRepository)
        {
        }

        public override bool HandlePacket(IPacket packet)
        {
            return HandlePaperdollPacket(packet, itemUnequipped: true);
        }
    }
}
