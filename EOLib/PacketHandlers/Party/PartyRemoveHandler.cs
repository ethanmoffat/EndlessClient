using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Party;
using EOLib.Net;
using EOLib.Net.Handlers;
using Optional.Collections;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Party
{
    /// <summary>
    /// Handles removing a member from the party
    /// </summary>
    [AutoMappedType]
    public class PartyRemoveHandler : InGameOnlyPacketHandler
    {
        private readonly IPartyDataRepository _partyDataRepository;
        private readonly IEnumerable<IPartyEventNotifier> _partyEventNotifiers;

        public override PacketFamily Family => PacketFamily.Party;

        public override PacketAction Action => PacketAction.Remove;

        public PartyRemoveHandler(IPlayerInfoProvider playerInfoProvider,
                                  IPartyDataRepository partyDataRepository,
                                  IEnumerable<IPartyEventNotifier> partyEventNotifiers)
            : base(playerInfoProvider)
        {
            _partyDataRepository = partyDataRepository;
            _partyEventNotifiers = partyEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var memberToRemove = packet.ReadShort();

            _partyDataRepository.Members.SingleOrNone(x => x.CharacterID == memberToRemove)
                .MatchSome(x =>
                {
                    _partyDataRepository.Members.Remove(x);
                    foreach (var notifier in _partyEventNotifiers)
                        notifier.NotifyPartyMemberRemove(x.Name);
                });

            return true;
        }
    }
}
