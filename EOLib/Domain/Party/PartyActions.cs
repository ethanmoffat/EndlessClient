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

        public void RequestParty(PartyRequestType type, short targetCharacterId)
        {
            var packet = new PacketBuilder(PacketFamily.Party, PacketAction.Request)
                .AddChar((byte)type)
                .AddShort(targetCharacterId)
                .Build();

            _packetSendService.SendPacket(packet);
        }

        public void AcceptParty(PartyRequestType type, short targetCharacterId)
        {
            var packet = new PacketBuilder(PacketFamily.Party, PacketAction.Accept)
                .AddChar((byte)type)
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

        public void RemovePartyMember(short targetCharacterId)
        {
            var packet = new PacketBuilder(PacketFamily.Party, PacketAction.Remove)
                .AddShort(targetCharacterId)
                .Build();

            _packetSendService.SendPacket(packet);
        }
    }

    public interface IPartyActions
    {
        void RequestParty(PartyRequestType type, short targetCharacterId);

        void AcceptParty(PartyRequestType type, short targetCharacterId);

        void ListParty();

        void RemovePartyMember(short targetCharacterId);
    }
}
