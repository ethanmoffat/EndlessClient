using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Map;
using EOLib.Domain.Party;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional.Collections;

namespace EOLib.PacketHandlers.Party
{
    /// <summary>
    /// Handles HP update for party members
    /// </summary>
    [AutoMappedType]
    public class PartyAgreeHandler : InGameOnlyPacketHandler<PartyAgreeServerPacket>
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

        public override bool HandlePacket(PartyAgreeServerPacket packet)
        {
            _partyDataRepository.Members.SingleOrNone(x => x.CharacterID == packet.PlayerId)
                .Match(
                    some: x =>
                    {
                        _partyDataRepository.Members.Remove(x);
                        _partyDataRepository.Members.Add(x.WithPercentHealth(packet.HpPercentage));
                    },
                    none: () => _currentMapStateProvider.UnknownPlayerIDs.Add(packet.PlayerId));

            return true;
        }
    }
}
