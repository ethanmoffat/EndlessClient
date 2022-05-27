using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Protocol;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional.Collections;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Init
{
    [AutoMappedType]
    public class InGameInitPacketHandler : InGameOnlyPacketHandler
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

        public override bool HandlePacket(IPacket packet)
        {
            var reply = (InitReply)packet.ReadByte();
            return _initPacketHandlers.SingleOrNone(x => x.Reply == reply)
                .Match(x => x.HandlePacket(packet), () => false);
        }
    }
}
