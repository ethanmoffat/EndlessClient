using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Party;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional.Collections;

namespace EOLib.PacketHandlers.Party
{
    /// <summary>
    /// Handles removing a member from the party
    /// </summary>
    [AutoMappedType]
    public class PartyRemoveHandler : InGameOnlyPacketHandler
    {
        private readonly IPartyDataRepository _partyDataRepository;

        public override PacketFamily Family => PacketFamily.Party;

        public override PacketAction Action => PacketAction.Remove;

        public PartyRemoveHandler(IPlayerInfoProvider playerInfoProvider,
                                  IPartyDataRepository partyDataRepository)
            : base(playerInfoProvider)
        {
            _partyDataRepository = partyDataRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var memberToRemove = packet.ReadShort();

            _partyDataRepository.Members.SingleOrNone(x => x.CharacterID == memberToRemove)
                .MatchSome(x => _partyDataRepository.Members.Remove(x));

            return true;
        }
    }
}
