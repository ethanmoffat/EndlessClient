using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Party;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Party
{
    /// <summary>
    /// Handles new member joining party
    /// </summary>
    [AutoMappedType]
    public class PartyAddHandler : InGameOnlyPacketHandler<PartyAddServerPacket>
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

        public override bool HandlePacket(PartyAddServerPacket packet)
        {
            _partyDataRepository.Members.Add(new Domain.Party.PartyMember.Builder
            {
                CharacterID = packet.Member.PlayerId,
                IsLeader = packet.Member.Leader,
                Level = packet.Member.Level,
                PercentHealth = packet.Member.HpPercentage,
                Name = char.ToUpper(packet.Member.Name[0]) + packet.Member.Name.Substring(1),
            }.ToImmutable());

            foreach (var notifier in _partyEventNotifiers)
            {
                notifier.NotifyPartyMemberAdd(packet.Member.Name);
            }

            return true;
        }
    }
}