using System.Collections.Generic;

using AutomaticTypeMapper;

using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;

using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Walk
{
    [AutoMappedType]
    public class WalkOpenHandler : InGameOnlyPacketHandler<WalkOpenServerPacket>
    {
        private readonly ICharacterRepository _characterRepository;
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.Walk;

        public override PacketAction Action => PacketAction.Open;

        public WalkOpenHandler(IPlayerInfoProvider playerInfoProvider,
                               ICharacterRepository characterRepository,
                               IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers)
            : base(playerInfoProvider)
        {
            _characterRepository = characterRepository;
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
        }

        public override bool HandlePacket(WalkOpenServerPacket packet)
        {
            _characterRepository.MainCharacter = _characterRepository.MainCharacter.WithFrozen(false);

            foreach (var notifier in _mainCharacterEventNotifiers)
                notifier.NotifyFrozen();

            return true;
        }
    }
}
