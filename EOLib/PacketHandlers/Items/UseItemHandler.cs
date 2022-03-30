using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.IO;
using EOLib.IO.Extensions;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.PacketHandlers.Items
{
    [AutoMappedType]
    public class UseItemHandler : InGameOnlyPacketHandler
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

        public UseItemHandler(IPlayerInfoProvider playerInfoProvider,
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

        public override bool HandlePacket(IPacket packet)
        {
            var itemType = (ItemType)packet.ReadChar();
            var itemId = packet.ReadShort();
            var amount = packet.ReadInt();
            var weight = packet.ReadChar();
            var maxWeight = packet.ReadChar();

            var oldItem = _characterInventoryRepository.ItemInventory.SingleOrNone(x => x.ItemID == itemId);
            oldItem.MatchSome(item => _characterInventoryRepository.ItemInventory.Remove(item));
            if (amount > 0)
                _characterInventoryRepository.ItemInventory.Add(new InventoryItem(itemId, amount));

            var character = _characterRepository.MainCharacter;
            var stats = character.Stats;
            stats = stats.WithNewStat(CharacterStat.Weight, weight).WithNewStat(CharacterStat.MaxWeight, maxWeight);

            var renderProps = character.RenderProperties;
            switch (itemType)
            {
                case ItemType.Teleport: break; // no-op: Warp handles the rest
                case ItemType.Heal:
                    var hpGain = packet.ReadInt();
                    var hp = packet.ReadShort();
                    var tp = packet.ReadShort();

                    stats = stats.WithNewStat(CharacterStat.HP, hp).WithNewStat(CharacterStat.TP, tp);
                    var percentHealth = (int)Math.Round(100.0 * (double)stats[CharacterStat.HP] / stats[CharacterStat.MaxHP]);

                    foreach (var notifier in _mainCharacterEventNotifiers)
                        notifier.NotifyTakeDamage(hpGain, percentHealth, isHeal: true);

                    break;
                case ItemType.HairDye:
                    var hairColor = packet.ReadChar();
                    renderProps = renderProps.WithHairColor(hairColor);
                    break;
                case ItemType.Beer:
                    // todo: drunk
                    // old logic: 
                    //   OldWorld.Instance.ActiveCharacterRenderer.MakeDrunk();
                    //   m_game.Hud.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.STATUS_LABEL_ITEM_USE_DRUNK);
                    break;
                case ItemType.EffectPotion:
                    var potionId = packet.ReadShort();

                    foreach (var notifier in _effectNotifiers)
                        notifier.NotifyPotionEffect((short)character.ID, potionId);

                    break;
                case ItemType.CureCurse:
                    var cureCurseMaxHp = packet.ReadShort();
                    var cureCurseMaxTp = packet.ReadShort();
                    var cureCurseStr = packet.ReadShort();
                    var cureCurseInt = packet.ReadShort();
                    var cureCurseWis = packet.ReadShort();
                    var cureCurseAgi = packet.ReadShort();
                    var cureCurseCon = packet.ReadShort();
                    var cureCurseCha = packet.ReadShort();
                    var cureCurseMinDam = packet.ReadShort();
                    var cureCurseMaxDam = packet.ReadShort();
                    var cureCurseAcc = packet.ReadShort();
                    var cureCurseEvade = packet.ReadShort();
                    var cureCurseArmor = packet.ReadShort();

                    if (_paperdollRepository.VisibleCharacterPaperdolls.ContainsKey(character.ID))
                    {
                        var paperdoll = _paperdollRepository.VisibleCharacterPaperdolls[character.ID].Paperdoll.ToDictionary(k => k.Key, v => v.Value);
                        for (EquipLocation loc = 0; loc < EquipLocation.PAPERDOLL_MAX; loc++)
                        {
                            var dollItem = paperdoll[loc];
                            if (dollItem <= 0)
                                continue;

                            var rec = _itemFileProvider.EIFFile[dollItem];
                            if (rec.Special == ItemSpecial.Cursed)
                            {
                                paperdoll[loc] = 0;

                                if (loc == EquipLocation.Boots)
                                    renderProps = renderProps.WithBootsGraphic(0);
                                else if (loc == EquipLocation.Armor)
                                    renderProps = renderProps.WithArmorGraphic(0);
                                else if (loc == EquipLocation.Hat)
                                    renderProps = renderProps.WithHatGraphic(0);
                                else if (loc == EquipLocation.Weapon)
                                    renderProps = renderProps.WithWeaponGraphic(0, false);
                                else if (loc == EquipLocation.Shield)
                                    renderProps = renderProps.WithShieldGraphic(0);
                            }
                        }

                        _paperdollRepository.VisibleCharacterPaperdolls[character.ID] =
                            _paperdollRepository.VisibleCharacterPaperdolls[character.ID].WithPaperdoll(paperdoll);
                    }

                    stats = stats.WithNewStat(CharacterStat.MaxHP, cureCurseMaxHp)
                        .WithNewStat(CharacterStat.MaxTP, cureCurseMaxTp)
                        .WithNewStat(CharacterStat.Strength, cureCurseStr)
                        .WithNewStat(CharacterStat.Intelligence, cureCurseInt)
                        .WithNewStat(CharacterStat.Wisdom, cureCurseWis)
                        .WithNewStat(CharacterStat.Agility, cureCurseAgi)
                        .WithNewStat(CharacterStat.Constituion, cureCurseCon)
                        .WithNewStat(CharacterStat.Charisma, cureCurseCha)
                        .WithNewStat(CharacterStat.MinDam, cureCurseMinDam)
                        .WithNewStat(CharacterStat.MaxDam, cureCurseMaxDam)
                        .WithNewStat(CharacterStat.Accuracy, cureCurseAcc)
                        .WithNewStat(CharacterStat.Evade, cureCurseEvade)
                        .WithNewStat(CharacterStat.Armor, cureCurseArmor);

                    break;
                case ItemType.EXPReward:  // todo: EXPReward has not been tested
                    var levelUpExp = packet.ReadInt();
                    var levelUpLevel = packet.ReadChar();
                    var levelUpStat = packet.ReadShort();
                    var levelUpSkill = packet.ReadShort();
                    var levelUpMaxHp = packet.ReadShort();
                    var levelUpMaxTp = packet.ReadShort();
                    var levelUpMaxSp = packet.ReadShort();

                    if (stats[CharacterStat.Level] < levelUpLevel)
                    {
                        foreach (var notifier in _emoteNotifiers)
                        {
                            notifier.NotifyEmote((short)_characterRepository.MainCharacter.ID, Emote.LevelUp);
                        }

                        stats = stats.WithNewStat(CharacterStat.Level, levelUpLevel);
                    }

                    stats = stats.WithNewStat(CharacterStat.Experience, levelUpExp)
                        .WithNewStat(CharacterStat.StatPoints, levelUpStat)
                        .WithNewStat(CharacterStat.SkillPoints, levelUpSkill)
                        .WithNewStat(CharacterStat.MaxHP, levelUpMaxHp)
                        .WithNewStat(CharacterStat.MaxTP, levelUpMaxTp)
                        .WithNewStat(CharacterStat.MaxSP, levelUpMaxSp);

                    break;
            }

            _characterRepository.MainCharacter = character.WithStats(stats).WithRenderProperties(renderProps);

            return true;
        }
    }
}
