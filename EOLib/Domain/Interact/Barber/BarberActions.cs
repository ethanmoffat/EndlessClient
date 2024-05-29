using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;
using System.Diagnostics;
using EOLib.Domain.Character;
namespace EOLib.Domain.Interact.Barber
{
    [AutoMappedType]
    public class BarberActions : IBarberActions
    {
        private readonly IPacketSendService _packetSendService;
        private readonly IBarberDataRepository _barberDataRepository;

        public BarberActions(IPacketSendService packetSendService,
                             IBarberDataRepository barberDataRepository)
        {
            _packetSendService = packetSendService;
            _barberDataRepository = barberDataRepository;
        }

        public void SayHello(int hairStyle, int hairColor)
        {
            var packet = new PacketBuilder(PacketFamily.Barber, PacketAction.Buy)
                .AddChar((char)hairStyle)
                .AddChar((char)hairColor)
                .AddInt(_barberDataRepository.SessionID) 
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface IBarberActions
    {
        void SayHello(int hairStyle, int hairColor);
    }
}