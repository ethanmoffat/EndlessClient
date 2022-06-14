using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Party;
using EOLib.Net;
using EOLib.Net.Handlers;
using System.Collections.Generic;

namespace EOLib.PacketHandlers.Party
{
    /// <summary>
    /// Handles new member joining party
    /// </summary>
    [AutoMappedType]
    public class PartyAddHandler : InGameOnlyPacketHandler
    {
        private readonly IPartyDataRepository _partyDataRepository;
        private readonly IEnumerable<IPartyEventNotifier> _partyEventNotifiers;

        public override PacketFamily Family => PacketFamily.Party;

        public override PacketAction Action => PacketAction.Add;

        public PartyAddHandler(IPlayerInfoProvider playerInfoProvider,
                               IPartyDataRepository partyDataRepository,
                               IEnumerable<IPartyEventNotifier> partyEventNotifiers)
            : base(playerInfoProvider)
        {
            _partyDataRepository = partyDataRepository;
            _partyEventNotifiers = partyEventNotifiers;
        }

        public override bool HandlePacket(IPacket packet)
        {
            var partyMember = new PartyMember.Builder
            {
                CharacterID = packet.ReadShort(),
                IsLeader = packet.ReadChar() != 0,
                Level = packet.ReadChar(),
                PercentHealth = packet.ReadChar(),
                Name = packet.ReadBreakString(),
            };
            partyMember.Name = char.ToUpper(partyMember.Name[0]) + partyMember.Name.Substring(1);

            _partyDataRepository.Members.Add(partyMember.ToImmutable());

            foreach (var notifier in _partyEventNotifiers)
            {
                notifier.NotifyPartyMemberAdd(partyMember.Name);
            }

            return true;
        }
    }
}
