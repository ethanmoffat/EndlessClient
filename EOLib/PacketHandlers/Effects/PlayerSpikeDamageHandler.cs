using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Extensions;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Effects
{
    [AutoMappedType]
    public class PlayerSpikeDamageHandler : InGameOnlyPacketHandler<EffectAdminServerPacket>
    {
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IOtherCharacterEventNotifier> _otherCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.Effect;

        public override PacketAction Action => PacketAction.Admin;

        public PlayerSpikeDamageHandler(IPlayerInfoProvider playerInfoProvider,
                                        ICurrentMapStateRepository currentMapStateRepository,
                                        IEnumerable<IOtherCharacterEventNotifier> otherCharacterEventNotifiers)
            : base(playerInfoProvider)
        {
            _currentMapStateRepository = currentMapStateRepository;
            _otherCharacterEventNotifiers = otherCharacterEventNotifiers;
        }

        public override bool HandlePacket(EffectAdminServerPacket packet)
        {
            if (_currentMapStateRepository.Characters.TryGetValue(packet.PlayerId, out var character))
            {
                var updatedCharacter = character.WithDamage(packet.Damage, packet.Died);
                _currentMapStateRepository.Characters.Update(character, updatedCharacter);

                foreach (var notifier in _otherCharacterEventNotifiers)
                {
                    notifier.OtherCharacterTakeDamage(packet.PlayerId, packet.HpPercentage, packet.Damage, isHeal: false);
                }
            }
            else
            {
                _currentMapStateRepository.UnknownPlayerIDs.Add(packet.PlayerId);
            }

            return true;
        }
    }
}
