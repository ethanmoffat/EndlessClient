using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

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
            var packet = new BookRequestClientPacket { PlayerId = characterId };
            _packetSendService.SendPacket(packet);
        }
    }

    public interface IBookActions
    {
        void RequestBook(int characterId);
    }
}
