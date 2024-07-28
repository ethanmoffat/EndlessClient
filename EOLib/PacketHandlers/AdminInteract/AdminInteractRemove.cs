using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.AdminInteract
{
    /// <summary>
    /// Admin hiding
    /// </summary>
    [AutoMappedType]
    public class AdminInteractRemove : InGameOnlyPacketHandler<AdminInteractRemoveServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateRepository _currentMapStateRepository;
        private readonly IEnumerable<IEffectNotifier> _effectNotifiers;

        public override PacketFamily Family => PacketFamily.AdminInteract;
        public override PacketAction Action => PacketAction.Remove;

        public AdminInteractRemove(IPlayerInfoProvider playerInfoProvider,
                                  ICharacterRepository characterRepository,
                                  ICurrentMapStateRepository currentMapStateRepository,
                                  IEnumerable<IEffectNotifier> effectNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _currentMapStateRepository = currentMapStateRepository;
            _effectNotifiers = effectNotifiers;
        }

        public override bool HandlePacket(AdminInteractRemoveServerPacket packet)
        {
            if (packet.PlayerId == _characterRepository.MainCharacter.ID)
            {
                _characterRepository.MainCharacter = Hidden(_characterRepository.MainCharacter);
            }
            else
            {
                if (_currentMapStateRepository.Characters.TryGetValue(packet.PlayerId, out var character))
                {
                    var updatedCharacter = Hidden(character);
                    _currentMapStateRepository.Characters.Update(character, updatedCharacter);
                }
                else
                {
                    _currentMapStateRepository.UnknownPlayerIDs.Add(packet.PlayerId);
                    return true;
                }
            }

            foreach (var notifier in _effectNotifiers)
                notifier.NotifyAdminHideEffect(packet.PlayerId);

            return true;
        }

        private static Character Hidden(Character input)
        {
            var renderProps = input.RenderProperties.WithIsHidden(true);
            return input.WithRenderProperties(renderProps);
        }
    }
}