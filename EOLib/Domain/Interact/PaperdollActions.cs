using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Interact
{
    [AutoMappedType]
    public class PaperdollActions : IPaperdollActions
    {
        private readonly IPacketSendService _packetSendService;

        public PaperdollActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void RequestPaperdoll(int characterId)
        {
            var packet = new PacketBuilder(PacketFamily.PaperDoll, PacketAction.Request)
                .AddShort(characterId)
                .Build();
            _packetSendService.SendPacket(packet);
        }
    }

    public interface IPaperdollActions
    {
        void RequestPaperdoll(int characterId);
    }
}
