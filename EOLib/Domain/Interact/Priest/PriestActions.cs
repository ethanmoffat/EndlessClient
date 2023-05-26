using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Interact.Priest
{
    [AutoMappedType]
    public class PriestActions : IPriestActions
    {
        private readonly IPriestSessionProvider _priestSessionProvider;
        private readonly IPacketSendService _packetSendService;

        public PriestActions(IPriestSessionProvider priestSessionProvider,
                             IPacketSendService packetSendService)
        {
            _priestSessionProvider = priestSessionProvider;
            _packetSendService = packetSendService;
        }

        public void AcceptRequest()
        {
            var packet = new PacketBuilder(PacketFamily.Priest, PacketAction.Accept)
                .AddShort(_priestSessionProvider.SessionID)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void RequestMarriage(string partner)
        {
            var packet = new PacketBuilder(PacketFamily.Priest, PacketAction.Request)
                .AddInt(_priestSessionProvider.SessionID)
                .AddByte(255)
                .AddString(partner)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void ConfirmMarriage()
        {
            var packet = new PacketBuilder(PacketFamily.Priest, PacketAction.Use)
                .AddInt(_priestSessionProvider.SessionID)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface IPriestActions
    {
        void AcceptRequest();

        void RequestMarriage(string partner);

        void ConfirmMarriage();
    }
}
