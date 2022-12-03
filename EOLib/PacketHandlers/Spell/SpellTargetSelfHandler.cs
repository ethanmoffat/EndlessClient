using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Spell
{
    /// <summary>
    /// Sent when a player cast a spell targeting themselves
    /// </summary>
    [AutoMappedType]
    public class SpellTargetSelfHandler : InGameOnlyPacketHandler
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly IEnumerable<IOtherCharacterAnimationNotifier> _animationNotifiers;

        public override PacketFamily Family => PacketFamily.Spell;
        public override PacketAction Action => PacketAction.TargetSelf;

        public SpellTargetSelfHandler(IPlayerInfoProvider playerInfoProvider,
                                      ICharacterRepository characterRepository,
                                      IEnumerable<IOtherCharacterAnimationNotifier> animationNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _animationNotifiers = animationNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            short fromPlayerID = packet.ReadShort();
            short spellID = packet.ReadShort();
            int spellHP = packet.ReadInt();
            byte percentHealth = packet.ReadChar();

            if (packet.ReadPosition != packet.Length)
            {
                //main player was source of this packet (otherwise, other player was source)
                short characterHP = packet.ReadShort();
                short characterTP = packet.ReadShort();
                if (packet.ReadShort() != 1) //malformed packet! eoserv sends '1' here
                    return false;

                var stats = _characterRepository.MainCharacter.Stats;
                stats = stats.WithNewStat(CharacterStat.HP, characterHP)
                             .WithNewStat(CharacterStat.TP, characterTP);

                _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithStats(stats);
            }

            foreach (var notifier in _animationNotifiers)
                notifier.NotifySelfSpellCast(fromPlayerID, spellID, spellHP, percentHealth);

            return true;
        }
    }
}
