using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
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

        public void Purchase(int hairStyle, int hairColor)
        {
            _packetSendService.SendPacket(new BarberBuyClientPacket
            {
                SessionId = _barberDataRepository.SessionID,
                HairStyle = hairStyle,
                HairColor = hairColor,
            });
        }
    }

    public interface IBarberActions
    {
        void Purchase(int hairStyle, int hairColor);
    }
}
