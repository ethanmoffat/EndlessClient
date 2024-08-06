using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

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
            var packet = new PriestAcceptClientPacket { SessionId = _priestSessionProvider.SessionID };
            _packetSendService.SendPacket(packet);
        }

        public void RequestMarriage(string partner)
        {
            var packet = new PriestRequestClientPacket { SessionId = _priestSessionProvider.SessionID, Name = partner };
            _packetSendService.SendPacket(packet);
        }

        public void ConfirmMarriage()
        {
            var packet = new PriestUseClientPacket { SessionId = _priestSessionProvider.SessionID };
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
