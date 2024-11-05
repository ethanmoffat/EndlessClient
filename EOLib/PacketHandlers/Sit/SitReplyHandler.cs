using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Sit
{
    /// <summary>
    /// Handle your player sitting
    /// </summary>
    [AutoMappedType]
    public class SitReplyHandler : PlayerSitHandlerBase<SitReplyServerPacket>
    {
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.Sit;

        public override PacketAction Action => PacketAction.Reply;

        public SitReplyHandler(IPlayerInfoProvider playerInfoProvider,
                               ICharacterRepository characterRepository,
                               ICurrentMapStateRepository currentMapStateRepository,
                               IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository)
        {
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
        }

        public override bool HandlePacket(SitReplyServerPacket packet)
        {
            Handle(packet.PlayerId, packet.Coords.X, packet.Coords.Y, (EODirection)packet.Direction);

            foreach (var notifier in _mainCharacterEventNotifiers)
                notifier.NotifySit();

            return true;
        }
    }
}
