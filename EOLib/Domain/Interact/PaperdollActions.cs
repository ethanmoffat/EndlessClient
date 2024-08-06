using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

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
            var packet = new PaperdollRequestClientPacket { PlayerId = characterId };
            _packetSendService.SendPacket(packet);
        }
    }

    public interface IPaperdollActions
    {
        void RequestPaperdoll(int characterId);
    }
}
