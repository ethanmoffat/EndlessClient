using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class MapDebuffHandler : InGameOnlyPacketHandler
    {
        private const int EFFECT_DAMAGE_TPDRAIN = 1;
        private const int EFFECT_DAMAGE_SPIKE = 2;

        private readonly ICharacterRepository _characterRepository;

        public override PacketFamily Family => PacketFamily.Effect;

        public override PacketAction Action => PacketAction.Spec;

        public MapDebuffHandler(IPlayerInfoProvider playerInfoProvider,
                                ICharacterRepository characterRepository)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
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
                    }
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
