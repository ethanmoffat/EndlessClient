using AutomaticTypeMapper;
using EOLib.Domain.Map;
using EOLib.Net;
using EOLib.Net.Communication;
using System.Collections.Generic;

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
            var packet = new PacketBuilder(PacketFamily.Citizen, PacketAction.Request)
                .AddShort(_currentMapStateProvider.MapWarpSession.ValueOr(0))
                .AddShort(_citizenDataProvider.BehaviorID.ValueOr(0))
                .Build();

            _packetSendService.SendPacket(packet);

        }

        public void ConfirmSleep()
        {
            var packet = new PacketBuilder(PacketFamily.Citizen, PacketAction.Accept)
                .AddShort(_currentMapStateProvider.MapWarpSession.ValueOr(0))
                .AddShort(_citizenDataProvider.BehaviorID.ValueOr(0))
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void SignUp(IReadOnlyList<string> answers)
        {
            var packet = new PacketBuilder(PacketFamily.Citizen, PacketAction.Reply)
                .AddShort(_currentMapStateProvider.MapWarpSession.ValueOr(0))
                .AddByte(255)
                .AddShort(_citizenDataProvider.BehaviorID.ValueOr(0))
                .AddByte(255)
                .AddBreakString(answers[0])
                .AddBreakString(answers[1])
                .AddString(answers[2])
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void Unsubscribe()
        {
            var packet = new PacketBuilder(PacketFamily.Citizen, PacketAction.Remove)
                .AddShort(_citizenDataProvider.BehaviorID.ValueOr(0))
                .Build();

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
