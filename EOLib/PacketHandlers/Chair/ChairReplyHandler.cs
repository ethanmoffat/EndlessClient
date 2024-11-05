using System.Collections;
using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.PacketHandlers.Sit;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Chair
{
    /// <summary>
    /// Handle your player sitting in a chair
    /// </summary>
    [AutoMappedType]
    public class ChairReplyHandler : PlayerSitHandlerBase<ChairReplyServerPacket>
    {
        private readonly IEnumerable<IMainCharacterEventNotifier> _mainCharacterEventNotifiers;

        public override PacketFamily Family => PacketFamily.Chair;

        public override PacketAction Action => PacketAction.Reply;

        public ChairReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                 ICharacterRepository characterRepository,
                                 ICurrentMapStateRepository currentMapStateRepository,
                                 IEnumerable<IMainCharacterEventNotifier> mainCharacterEventNotifiers)
            : base(playerInfoProvider, characterRepository, currentMapStateRepository)
        {
            _mainCharacterEventNotifiers = mainCharacterEventNotifiers;
        }

        public override bool HandlePacket(ChairReplyServerPacket packet)
        {
            Handle(packet.PlayerId, packet.Coords.X, packet.Coords.Y, (EODirection)packet.Direction);

            foreach (var notifier in _mainCharacterEventNotifiers)
                notifier.NotifySit();

            return true;
        }
    }
}
