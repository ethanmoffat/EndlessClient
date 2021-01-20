using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers
{
    [AutoMappedType]
    public class TimedSpikeEffectHandler : InGameOnlyPacketHandler
    {
        public override PacketFamily Family => PacketFamily.Effect;

        public override PacketAction Action => PacketAction.Report;

        public TimedSpikeEffectHandler(IPlayerInfoProvider playerInfoProvider)
            : base(playerInfoProvider) { }

        public override bool HandlePacket(IPacket packet)
        {
            if ((char)packet.ReadChar() != 'S')
                return false;

            // todo: play sound effect for timed spikes
            return true;
        }
    }
}
