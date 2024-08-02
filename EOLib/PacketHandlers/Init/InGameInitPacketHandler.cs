using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional.Collections;

namespace EOLib.PacketHandlers.Init
{
    [AutoMappedType]
    public class InGameInitPacketHandler : InGameOnlyPacketHandler<InitInitServerPacket>
    {
        private readonly IEnumerable<IInitPacketHandler> _initPacketHandlers;

        public override PacketFamily Family => PacketFamily.Init;

        public override PacketAction Action => PacketAction.Init;

        public InGameInitPacketHandler(IPlayerInfoProvider playerInfoProvider,
                                       IEnumerable<IInitPacketHandler> initPacketHandlers)
            : base(playerInfoProvider)
        {
            _initPacketHandlers = initPacketHandlers;
        }

        public override bool HandlePacket(InitInitServerPacket packet)
        {
            return _initPacketHandlers.SingleOrNone(x => x.Reply == packet.ReplyCode)
                .Match(x => x.HandleData(packet.ReplyCodeData), () => false);
        }
    }
}