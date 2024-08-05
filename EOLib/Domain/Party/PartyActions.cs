using AutomaticTypeMapper;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace EOLib.Domain.Party
{
    [AutoMappedType]
    public class PartyActions : IPartyActions
    {
        private readonly IPacketSendService _packetSendService;

        public PartyActions(IPacketSendService packetSendService)
        {
            _packetSendService = packetSendService;
        }

        public void RequestParty(PartyRequestType type, int targetCharacterId)
        {
            var packet = new PartyRequestClientPacket
            {
                RequestType = type,
                PlayerId = targetCharacterId
            };
            _packetSendService.SendPacket(packet);
        }

        public void AcceptParty(PartyRequestType type, int targetCharacterId)
        {
            var packet = new PartyAcceptClientPacket
            {
                RequestType = type,
                InviterPlayerId = targetCharacterId
            };
            _packetSendService.SendPacket(packet);
        }

        public void ListParty()
        {
            _packetSendService.SendPacket(new PartyTakeClientPacket());
        }

        public void RemovePartyMember(int targetCharacterId)
        {
            var packet = new PartyRemoveClientPacket
            {
                PlayerId = targetCharacterId,
            };
            _packetSendService.SendPacket(packet);
        }
    }

    public interface IPartyActions
    {
        void RequestParty(PartyRequestType type, int targetCharacterId);

        void AcceptParty(PartyRequestType type, int targetCharacterId);

        void ListParty();

        void RemovePartyMember(int targetCharacterId);
    }
}