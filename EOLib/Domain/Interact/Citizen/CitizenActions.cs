using AutomaticTypeMapper;
using EOLib.Domain.Map;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.Domain.Interact.Citizen
{
    [AutoMappedType]
    public class CitizenActions : ICitizenActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly ICitizenDataProvider _citizenDataProvider;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;

        public CitizenActions(IPacketSendService packetSendService,
                              ICitizenDataProvider citizenDataProvider,
                              ICurrentMapStateProvider currentMapStateProvider)
        {
            _packetSendService = packetSendService;
            _citizenDataProvider = citizenDataProvider;
            _currentMapStateProvider = currentMapStateProvider;
        }

        public void RequestSleep()
        {
            var packet = new CitizenRequestClientPacket
            {
                SessionId = _currentMapStateProvider.MapWarpSession.ValueOr(0),
                BehaviorId = _citizenDataProvider.BehaviorID.ValueOr(0),
            };
            _packetSendService.SendPacket(packet);

        }

        public void ConfirmSleep()
        {
            var packet = new CitizenAcceptClientPacket
            {
                SessionId = _currentMapStateProvider.MapWarpSession.ValueOr(0),
                BehaviorId = _citizenDataProvider.BehaviorID.ValueOr(0),
            };
            _packetSendService.SendPacket(packet);
        }

        public void SignUp(IReadOnlyList<string> answers)
        {
            var packet = new CitizenReplyClientPacket
            {
                SessionId = _currentMapStateProvider.MapWarpSession.ValueOr(0),
                BehaviorId = _citizenDataProvider.BehaviorID.ValueOr(0),
                Answers = answers.ToList(),
            };
            _packetSendService.SendPacket(packet);
        }

        public void Unsubscribe()
        {
            var packet = new CitizenRemoveClientPacket
            {
                BehaviorId = _citizenDataProvider.BehaviorID.ValueOr(0)
            };
            _packetSendService.SendPacket(packet);
        }
    }

    public interface ICitizenActions
    {
        void RequestSleep();

        void ConfirmSleep();

        void SignUp(IReadOnlyList<string> answers);

        void Unsubscribe();
    }
}
