using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.IO.Extensions;
using EOLib.IO.Repositories;
using EOLib.PacketHandlers.Avatar;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using Optional.Collections;
using System.Linq;

namespace EOLib.PacketHandlers.Paperdoll
{
    /// <summary>
    /// Base handler for Paperdoll equip/unequip
    /// </summary>
    public abstract class ItemEquipHandler<TPacket> : AvatarChangeHandler<TPacket>
        where TPacket : IPacket
    {
        private readonly IPaperdollRepository _paperdollRepository;
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly IEIFFileProvider _eifFileProvider;

        protected ItemEquipHandler(IPlayerInfoProvider playerInfoProvider,
                                   ICurrentMapStateRepository currentMapStateRepository,
                                   ICharacterRepository characterRepository,
                                   IPaperdollRepository paperdollRepository,
                                   ICharacterInventoryRepository characterInventoryRepository,
                                   IEIFFileProvider eifFileProvider)
            : base(playerInfoProvider, currentMapStateRepository, characterRepository)
        {
            _paperdollRepository = paperdollRepository;
            _characterInventoryRepository = characterInventoryRepository;
            _eifFileProvider = eifFileProvider;
        }

        protected bool HandlePaperdollPacket(AvatarChange change, int itemId, int amount, int subLoc, CharacterStatsEquipmentChange stats)
        {
            Handle(change);

            var itemUnequipped = Action == PacketAction.Remove;

            if (_paperdollRepository.VisibleCharacterPaperdolls.ContainsKey(change.PlayerId))
            {
                var paperdollData = _paperdollRepository.VisibleCharacterPaperdolls[change.PlayerId];

                var itemRec = _eifFileProvider.EIFFile[itemId];
                var paperdollSlot = itemRec.GetEquipLocation() + subLoc;

                var paperdollEquipData = paperdollData.Paperdoll.ToDictionary(k => k.Key, v => v.Value);
                paperdollEquipData[paperdollSlot] = itemUnequipped ? 0 : itemId;

                _paperdollRepository.VisibleCharacterPaperdolls[change.PlayerId] = paperdollData.WithPaperdoll(paperdollEquipData);
            }

            var update = _characterRepository.MainCharacter.ID == change.PlayerId
                ? Option.Some(_characterRepository.MainCharacter)
                : _currentMapStateRepository.Characters.TryGetValue(change.PlayerId, out var character)
                    ? Option.Some(character)
                    : Option.None<Character>();

            update.MatchSome(c =>
            {
                var characterStats = c.Stats
                    .WithNewStat(CharacterStat.MaxHP, stats.MaxHp)
                    .WithNewStat(CharacterStat.MaxTP, stats.MaxTp)
                    .WithNewStat(CharacterStat.Strength, stats.BaseStats.Str)
                    .WithNewStat(CharacterStat.Intelligence, stats.BaseStats.Intl)
                    .WithNewStat(CharacterStat.Wisdom, stats.BaseStats.Wis)
                    .WithNewStat(CharacterStat.Agility, stats.BaseStats.Agi)
                    .WithNewStat(CharacterStat.Constitution, stats.BaseStats.Con)
                    .WithNewStat(CharacterStat.Charisma, stats.BaseStats.Cha)
                    .WithNewStat(CharacterStat.MinDam, stats.SecondaryStats.MinDamage)
                    .WithNewStat(CharacterStat.MaxDam, stats.SecondaryStats.MaxDamage)
                    .WithNewStat(CharacterStat.Accuracy, stats.SecondaryStats.Accuracy)
                    .WithNewStat(CharacterStat.Evade, stats.SecondaryStats.Evade)
                    .WithNewStat(CharacterStat.Armor, stats.SecondaryStats.Armor);

                if (c == _characterRepository.MainCharacter)
                {
                    _characterRepository.MainCharacter = c.WithStats(characterStats);

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
                    _currentMapStateRepository.Characters.Update(c, c.WithStats(characterStats));
                }
            });

            update.MatchNone(() =>
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(change.PlayerId);
            });

            return true;
        }
    }
}
