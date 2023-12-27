using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EOLib.Domain.Interact
{
    [AutoMappedType]
    public class BookActions : IBookActions
    {
        private readonly IPacketSendService _packetSendService;

        public BookActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void RequestBook(int characterId)
        {
            var packet = new PacketBuilder(PacketFamily.Book, PacketAction.Request)
                .AddShort(characterId)
                .Build();
            _packetSendService.SendPacket(packet);
        }
    }

    public interface IBookActions
    {
        void RequestBook(int characterId);
    }
}
