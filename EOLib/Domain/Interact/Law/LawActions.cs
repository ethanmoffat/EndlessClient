using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Interact.Law
{
    [AutoMappedType]
    public class LawActions : ILawActions
    {
        private readonly ILawSessionProvider _lawSessionProvider;
        private readonly IPacketSendService _packetSendService;

        public LawActions(ILawSessionProvider lawSessionProvider,
                          IPacketSendService packetSendService)
        {
            _lawSessionProvider = lawSessionProvider;
            _packetSendService = packetSendService;
        }

        public void RequestMarriage(string partner)
        {
            SendShared(MarriageRequestType.Marriage, partner);
        }

        public void RequestDivorce(string partner)
        {
            SendShared(MarriageRequestType.Divorce, partner);
        }

        private void SendShared(MarriageRequestType requestType, string partner)
        {
            var packet = new PacketBuilder(PacketFamily.Marriage, PacketAction.Request)
                .AddChar((int)requestType)
                .AddInt(_lawSessionProvider.SessionID)
                .AddByte(255)
                .AddString(partner)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface ILawActions
    {
        void RequestMarriage(string partner);

        void RequestDivorce(string partner);
    }
}
