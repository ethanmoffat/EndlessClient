using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.IO.Repositories;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Moffat.EndlessOnline.SDK.Protocol.Pub;
using Optional.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers.Items
{
    /// <summary>
    /// Sent when the main character uses an item
    /// </summary>
    [AutoMappedType]
    public class ItemReplyHandler : InGameOnlyPacketHandler<ItemReplyServerPacket>
    {
        private readonly ICharacterInventoryRepository _characterInventoryRepository;
        private readonly ICharacterRepository _characterRepository;
        private readonly IPaperdollRepository _paperdollRepository;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;
        private readonly IEnumerable<IEmoteNotifier> _emoteNotifiers;
        private readonly IEIFFileProvider _itemFileProvider;

        public override PacketFamily Family => PacketFamily.Item;

        public override PacketAction Action => PacketAction.Reply;

        public ItemReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterInventoryRepository characterInventoryRepository,
                                ICharacterRepository characterRepository,
                                IPaperdollRepository paperdollRepository,
                                IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers,
                                IEnumerable<IEffectNotifier> effectNotifiers,
                                IEnumerable<IEmoteNotifier> emoteNotifiers,
                                IEIFFileProvider itemFileProvider)
            : base(playerInfoProvider)
        {
            _characterInventoryRepository = characterInventoryRepository;
            _characterRepository = characterRepository;
            _paperdollRepository = paperdollRepository;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
            _effectNotifiers = effectNotifiers;
            _emoteNotifiers = emoteNotifiers;
            _itemFileProvider = itemFileProvider;
        }

        public override bool HandlePacket(ItemReplyServerPacket packet)
        {
            var oldItem = _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == packet.UsedItem.Id);
            oldItem.MatchSome(item => _characterInventoryRepository.ItemInventory.Remove(item));
            if (packet.UsedItem.Amount > 0)
                _characterInventoryRepository.ItemInventory.Add(new InventoryItem(packet.UsedItem.Id, packet.UsedItem.Amount));

            var character = _characterRepository.MainCharacter;
            var stats = character.Stats;
            stats = stats.WithNewStat(CharacterStat.Weight, packet.Weight.Current).WithNewStat(CharacterStat.MaxWeight, packet.Weight.Max);

            var renderProps = character.RenderProperties;
            switch (packet.ItemType)
            {
                case ItemType.Teleport: break; // no-op: Warp handles the rest
                case ItemType.Heal:
                    {
                        var data = (ItemReplyServerPacket.ItemTypeDataHeal)packet.ItemTypeData;

                        stats = stats.WithNewStat(CharacterStat.HP, data.Hp).WithNewStat(CharacterStat.TP, data.Tp);
                        var percentHealth = (int)Math.Round(100.0 * stats[CharacterStat.HP] / stats[CharacterStat.MaxHP]);

                        foreach (var notifier in _mainCharacterEventNotifiers)
                            notifier.NotifyTakeDamage(data.HpGain, percentHealth, isHeal: true);
                    }
                    break;
                case ItemType.HairDye:
                    {
                        var data = (ItemReplyServerPacket.ItemTypeDataHairDye)packet.ItemTypeData;
                        renderProps = renderProps.WithHairColor(data.HairColor);
                    }
                    break;
                case ItemType.Alcohol:
                    renderProps = renderProps.WithIsDrunk(true);
                    foreach (var notifier in _emoteNotifiers)
                        notifier.MakeMainPlayerDrunk();
                    break;
                case ItemType.EffectPotion:
                    {
                        var data = (ItemReplyServerPacket.ItemTypeDataEffectPotion)packet.ItemTypeData;
                        foreach (var notifier in _effectNotifiers)
                            notifier.NotifyPotionEffect(character.ID, data.EffectId);
                    }
                    break;
                case ItemType.CureCurse:
                    {
                        var data = (ItemReplyServerPacket.ItemTypeDataCureCurse)packet.ItemTypeData;

                        if (_paperdollRepository.VisibleCharacterPaperdolls.ContainsKey(character.ID))
                        {
                            var paperdoll = _paperdollRepository.VisibleCharacterPaperdolls[character.ID].Paperdoll.ToDictionary(k => k.Key, v => v.Value);
                            for (IO.EquipLocation loc = 0; loc < IO.EquipLocation.PAPERDOLL_MAX; loc++)
                            {
                                var dollItem = paperdoll[loc];
                                if (dollItem <= 0)
                                    continue;

                                var rec = _itemFileProvider.EIFFile[dollItem];
                                if (rec.Special == IO.ItemSpecial.Cursed)
                                {
                                    paperdoll[loc] = 0;

                                    if (loc == IO.EquipLocation.Boots)
                                        renderProps = renderProps.WithBootsGraphic(0);
                                    else if (loc == IO.EquipLocation.Armor)
                                        renderProps = renderProps.WithArmorGraphic(0);
                                    else if (loc == IO.EquipLocation.Hat)
                                        renderProps = renderProps.WithHatGraphic(0);
                                    else if (loc == IO.EquipLocation.Weapon)
                                        renderProps = renderProps.WithWeaponGraphic(0);
                                    else if (loc == IO.EquipLocation.Shield)
                                        renderProps = renderProps.WithShieldGraphic(0);
                                }
                            }

                            _paperdollRepository.VisibleCharacterPaperdolls[character.ID] =
                                _paperdollRepository.VisibleCharacterPaperdolls[character.ID].WithPaperdoll(paperdoll);
                        }

                        stats = stats.WithNewStat(CharacterStat.MaxHP, data.Stats.MaxHp)
                            .WithNewStat(CharacterStat.MaxTP, data.Stats.MaxTp)
                            .WithNewStat(CharacterStat.Strength, data.Stats.BaseStats.Str)
                            .WithNewStat(CharacterStat.Intelligence, data.Stats.BaseStats.Intl)
                            .WithNewStat(CharacterStat.Wisdom, data.Stats.BaseStats.Wis)
                            .WithNewStat(CharacterStat.Agility, data.Stats.BaseStats.Agi)
                            .WithNewStat(CharacterStat.Constitution, data.Stats.BaseStats.Con)
                            .WithNewStat(CharacterStat.Charisma, data.Stats.BaseStats.Cha)
                            .WithNewStat(CharacterStat.MinDam, data.Stats.SecondaryStats.MinDamage)
                            .WithNewStat(CharacterStat.MaxDam, data.Stats.SecondaryStats.MaxDamage)
                            .WithNewStat(CharacterStat.Accuracy, data.Stats.SecondaryStats.Accuracy)
                            .WithNewStat(CharacterStat.Evade, data.Stats.SecondaryStats.Evade)
                            .WithNewStat(CharacterStat.Armor, data.Stats.SecondaryStats.Armor);
                    }
                    break;
                case ItemType.ExpReward:
                    {
                        var data = (ItemReplyServerPacket.ItemTypeDataExpReward)packet.ItemTypeData;

                        if (stats[CharacterStat.Level] < data.LevelUp)
                        {
                            foreach (var notifier in _emoteNotifiers)
                            {
                                notifier.NotifyEmote(_characterRepository.MainCharacter.ID, Domain.Character.Emote.LevelUp);
                            }

                            stats = stats.WithNewStat(CharacterStat.Level, data.LevelUp);
                        }

                        stats = stats.WithNewStat(CharacterStat.Experience, data.Experience)
                            .WithNewStat(CharacterStat.StatPoints, data.StatPoints)
                            .WithNewStat(CharacterStat.SkillPoints, data.SkillPoints)
                            .WithNewStat(CharacterStat.MaxHP, data.MaxHp)
                            .WithNewStat(CharacterStat.MaxTP, data.MaxTp)
                            .WithNewStat(CharacterStat.MaxSP, data.MaxSp);
                    }
                    break;
            }

            _characterRepository.MainCharacter = character.WithStats(stats).WithRenderProperties(renderProps);

            return true;
        }
    }
}