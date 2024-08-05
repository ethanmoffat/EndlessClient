using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Party;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using Optional.Collections;

namespace EOLib.PacketHandlers.Party
{
    /// <summary>
    /// Handles removing a member from the party
    /// </summary>
    [AutoMappedType]
    public class PartyRemoveHandler : InGameOnlyPacketHandler<PartyRemoveServerPacket>
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

        public override bool HandlePacket(PartyRemoveServerPacket packet)
        {
            _partyDataRepository.Members.SingleOrNone(x => x.CharacterID == packet.PlayerId)
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