using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional;
using System;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Effects
{
    [AutoMappedType]
    public class MapDebuffHandler : InGameOnlyPacketHandler<EffectSpecServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;

        public override PacketFamily Family => PacketFamily.Effect;

        public override PacketAction Action => PacketAction.Spec;

        public MapDebuffHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                ICurrentMapProvider currentMapProvider,
                                ICurrentMapStateRepository currentMapStateRepository,
                                IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers,
                                IEnumerable<IEffectNotifier> effectNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapProvider = currentMapProvider;
            _currentMapStateRepository = currentMapStateRepository;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
            _effectNotifiers = effectNotifiers;
        }

        public override bool HandlePacket(EffectSpecServerPacket packet)
        {
            var character = _characterRepository.MainCharacter;
            var originalStats = character.Stats;

            switch (packet.MapDamageType)
            {
                // todo: show amount in damage counter
                case MapDamageType.TpDrain:
                    {
                        var data = (EffectSpecServerPacket.MapDamageTypeDataTpDrain)packet.MapDamageTypeData;
                        _characterRepository.MainCharacter = character.WithStats(
                            originalStats.WithNewStat(CharacterStat.TP, data.Tp)
                                .WithNewStat(CharacterStat.MaxTP, data.MaxTp));

                        foreach (var notifier in _effectNotifiers)
                            notifier.NotifyMapEffect(IO.Map.MapEffect.TPDrain);
                    }
                    break;
                case MapDamageType.Spikes:
                    {
                        if (_currentMapProvider.CurrentMap.Tiles[character.RenderProperties.MapY, character.RenderProperties.MapX] == IO.Map.TileSpec.SpikesTimed)
                        {
                            _currentMapStateRepository.LastTimedSpikeEvent = Option.Some(DateTime.Now);
                            foreach (var notifier in _effectNotifiers)
                                notifier.NotifyMapEffect(IO.Map.MapEffect.Spikes);
                        }

                        var data = (EffectSpecServerPacket.MapDamageTypeDataSpikes)packet.MapDamageTypeData;
                        character = character.WithStats(originalStats.WithNewStat(CharacterStat.HP, data.Hp)
                                                                     .WithNewStat(CharacterStat.MaxHP, data.MaxHp));

                        character = character.WithRenderProperties(character.RenderProperties.WithIsDead(data.Hp <= 0));

                        _characterRepository.MainCharacter = character;

                        foreach (var notifier in _mainCharacterEventNotifiers)
                            notifier.NotifyTakeDamage(data.HpDamage, (int)Math.Round((double)data.Hp / data.MaxHp * 100), isHeal: false);
                    }
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}