using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

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
            SendShared(MarriageRequestType.MarriageApproval, partner);
        }

        public void RequestDivorce(string partner)
        {
            SendShared(MarriageRequestType.Divorce, partner);
        }

        private void SendShared(MarriageRequestType requestType, string partner)
        {
            var packet = new MarriageRequestClientPacket
            {
                RequestType = requestType,
                SessionId = _lawSessionProvider.SessionID,
                Name = partner
            };
            _packetSendService.SendPacket(packet);
        }
    }

    public interface ILawActions
    {
        void RequestMarriage(string partner);

        void RequestDivorce(string partner);
    }
}