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
    /// Handles HP update for party members
    /// </summary>
    [AutoMappedType]
    public class PartyCreateHandler : InGameOnlyPacketHandler<PartyCreateServerPacket>
    {
        private readonly IPartyDataRepository _partyDataRepository;
        private readonly IEnumerable<IPartyEventNotifier> _partyEventNotifiers;

        public override PacketFamily Family => PacketFamily.Party;

        public override PacketAction Action => PacketAction.Create;

        public PartyCreateHandler(IPlayerInfoProvider playerInfoProvider,
                                  IPartyDataRepository partyDataRepository,
                                  IEnumerable<IPartyEventNotifier> partyEventNotifiers)
            : base(playerInfoProvider)
        {
            _partyDataRepository = partyDataRepository;
            _partyEventNotifiers = partyEventNotifiers;
        }

        public override bool HandlePacket(PartyCreateServerPacket packet)
        {
            _partyDataRepository.Members.Clear();

            foreach (var member in packet.Members)
            {
                _partyDataRepository.Members.Add(new Domain.Party.PartyMember.Builder
                {
                    CharacterID = member.PlayerId,
                    IsLeader = member.Leader,
                    Level = member.Level,
                    PercentHealth = member.HpPercentage,
                    Name = char.ToUpper(member.Name[0]) + member.Name.Substring(1),
                }.ToImmutable());
            }

            foreach (var notifier in _partyEventNotifiers)
                notifier.NotifyPartyJoined();

            return true;
        }
    }
}