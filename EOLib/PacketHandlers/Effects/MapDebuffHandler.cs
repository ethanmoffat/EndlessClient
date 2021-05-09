using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Effects
{
    [AutoMappedType]
    public class MapDebuffHandler : InGameOnlyPacketHandler
    {
        private const int EFFECT_DAMAGE_TPDRAIN = 1;
        private const int EFFECT_DAMAGE_SPIKE = 2;

        private readonly ICharacterRepository _characterRepository;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.Effect;

        public override PacketAction Action => PacketAction.Spec;

        public MapDebuffHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository,
                                IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var character = _characterRepository.MainCharacter;
            var originalStats = character.Stats;

            //1 in eoserv Map::TimedDrains - tp
            //2 in eoserv Character::SpikeDamage
            var damageType = packet.ReadChar();
            switch (damageType)
            {
                case EFFECT_DAMAGE_TPDRAIN:
                    {
                        // todo: show amount in damage counter
                        var amount = packet.ReadShort();
                        var tp = packet.ReadShort();
                        var maxTp = packet.ReadShort();
                        _characterRepository.MainCharacter = character.WithStats(
                            originalStats.WithNewStat(CharacterStat.TP, tp)
                                .WithNewStat(CharacterStat.MaxTP, maxTp));
                    }
                    break;
                case EFFECT_DAMAGE_SPIKE:
                    {
                        // todo: show amount in damage counter
                        var damage = packet.ReadShort();
                        var hp = packet.ReadShort();
                        var maxHp = packet.ReadShort();
                        character = character.WithStats(originalStats.WithNewStat(CharacterStat.HP, hp)
                                                                     .WithNewStat(CharacterStat.MaxHP, maxHp));

                        if (hp <= 0)
                            character = character.WithRenderProperties(character.RenderProperties.WithDead());

                        _characterRepository.MainCharacter = character;

                        foreach (var notifier in _mainCharacterEventNotifiers)
                            notifier.NotifyTakeDamage(damage, (int)Math.Round((double)hp / maxHp) * 100, isHeal: false);
                    }
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
