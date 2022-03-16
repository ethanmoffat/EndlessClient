using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.IO;
using EOLib.IO.Repositories;
using EOLib.Net;
using EOLib.Net.Handlers;
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
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;
        private readonly IEIFFileProvider _itemFileProvider;

        public override PacketFamily Family => PacketFamily.Item;

        public override PacketAction Action => PacketAction.Reply;

        public UseItemHandler(IPlayerInfoProvider playerInfoProvider,
                              ICharacterInventoryRepository characterInventoryRepository,
                              ICharacterRepository characterRepository,
                              IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers,
                              IEnumerable<IEffectNotifier> effectNotifiers,
                              IEIFFileProvider itemFileProvider)
            : base(playerInfoProvider)
        {
            _characterInventoryRepository = characterInventoryRepository;
            _characterRepository = characterRepository;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
            _effectNotifiers = effectNotifiers;
            _itemFileProvider = itemFileProvider;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var itemType = (ItemType)packet.ReadChar();
            var itemId = packet.ReadShort();
            var amount = packet.ReadInt();
            var weight = packet.ReadChar();
            var maxWeight = packet.ReadChar();

            var oldItem = _characterInventoryRepository.ItemInventory.SingleOrDefault(x => x.ItemID == itemId);
            if (oldItem == null)
                return false;

            _characterInventoryRepository.ItemInventory.Remove(oldItem);
            _characterInventoryRepository.ItemInventory.Add(new InventoryItem(itemId, amount));

            var character = _characterRepository.MainCharacter;
            var stats = character.Stats;
            var renderProps = character.RenderProperties;
            stats = stats.WithNewStat(CharacterStat.Weight, weight).WithNewStat(CharacterStat.MaxWeight, maxWeight);

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

                    var cursedItems = _itemFileProvider.EIFFile.Where(x => x.Special == ItemSpecial.Cursed).ToList();
                    if (cursedItems.Any(x => x.Graphic == renderProps.BootsGraphic && x.Type == ItemType.Boots))
                        renderProps = renderProps.WithBootsGraphic(0);
                    if (cursedItems.Any(x => x.Graphic == renderProps.ArmorGraphic && x.Type == ItemType.Armor))
                        renderProps = renderProps.WithArmorGraphic(0);
                    if (cursedItems.Any(x => x.Graphic == renderProps.HatGraphic && x.Type == ItemType.Hat))
                        renderProps = renderProps.WithHatGraphic(0);
                    if (cursedItems.Any(x => x.Graphic == renderProps.ShieldGraphic && x.Type == ItemType.Shield))
                        renderProps = renderProps.WithShieldGraphic(0);
                    if (cursedItems.Any(x => x.Graphic == renderProps.WeaponGraphic && x.Type == ItemType.Weapon))
                        renderProps = renderProps.WithWeaponGraphic(0, isRanged: false);

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
                case ItemType.EXPReward:
                    var levelUpExp = packet.ReadInt();
                    var levelUpLevel = packet.ReadChar();
                    var levelUpStat = packet.ReadShort();
                    var levelUpSkill = packet.ReadShort();
                    var levelUpMaxHp = packet.ReadShort();
                    var levelUpMaxTp = packet.ReadShort();
                    var levelUpMaxSp = packet.ReadShort();

                    if (stats[CharacterStat.Level] < levelUpLevel)
                    {
                        foreach (var notifier in _effectNotifiers)
                        {
                            // todo: level up emote
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
