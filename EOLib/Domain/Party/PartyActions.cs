using AutomaticTypeMapper;
using EOLib.Net;
using EOLib.Net.Communication;

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
            var packet = new PacketBuilder(PacketFamily.Party, PacketAction.Request)
                .AddChar((int)type)
                .AddShort(targetCharacterId)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void AcceptParty(PartyRequestType type, int targetCharacterId)
        {
            var packet = new PacketBuilder(PacketFamily.Party, PacketAction.Accept)
                .AddChar((int)type)
                .AddShort(targetCharacterId)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void ListParty()
        {
            var packet = new PacketBuilder(PacketFamily.Party, PacketAction.Take)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void RemovePartyMember(int targetCharacterId)
        {
            var packet = new PacketBuilder(PacketFamily.Party, PacketAction.Remove)
                .AddShort(targetCharacterId)
                .Build();

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
