using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Party;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional.Collections;

namespace EOLib.PacketHandlers.Party
{
    /// <summary>
    /// Handles HP update for party members
    /// </summary>
    [AutoMappedType]
    public class PartyAgreeHandler : InGameOnlyPacketHandler
    {
        private readonly IPartyDataRepository _partyDataRepository;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;

        public override PacketFamily Family => PacketFamily.Party;

        public override PacketAction Action => PacketAction.Agree;

        public PartyAgreeHandler(IPlayerInfoProvider playerInfoProvider,
                                 IPartyDataRepository partyDataRepository,
                                 ICurrentMapStateProvider currentMapStateProvider)
            : base(playerInfoProvider)
        {
            _partyDataRepository = partyDataRepository;
            _currentMapStateProvider = currentMapStateProvider;
        }

        public override bool HandlePacket(IPacket packet)
        {
            while (packet.ReadPosition < packet.Length)
            {
                var playerId = packet.ReadShort();
                var percentHealth = packet.ReadChar();

                _partyDataRepository.Members.SingleOrNone(x => x.CharacterID == playerId)
                    .Match(
                        some: x =>
                        {
                            _partyDataRepository.Members.Remove(x);
                            _partyDataRepository.Members.Add(x.WithPercentHealth(percentHealth));
                        },
                        none: () => _currentMapStateProvider.UnknownPlayerIDs.Add(playerId));
            }

            return true;
        }
    }
}
