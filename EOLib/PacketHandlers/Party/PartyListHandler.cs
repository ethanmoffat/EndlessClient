using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Party;
using EOLib.Net;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Party
{
    /// <summary>
    /// Handles data update for party members
    /// </summary>
    [AutoMappedType]
    public class PartyListHandler : PartyCreateHandler
    {
        public override PacketAction Action => PacketAction.List;

        public PartyListHandler(IPlayerInfoProvider playerInfoProvider,
                                IPartyDataRepository partyDataRepository,
                                IEnumerable<IPartyEventNotifier> partyEventNotifiers)
            : base(playerInfoProvider, partyDataRepository, partyEventNotifiers) { }
    }
}
