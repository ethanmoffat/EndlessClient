using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Party;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Party
{
    /// <summary>
    /// Handles data update for party members
    /// </summary>
    [AutoMappedType]
    public class PartyListHandler : InGameOnlyPacketHandler<PartyListServerPacket>
    {
        private readonly IPartyDataRepository _partyDataRepository;

        public override PacketFamily Family => PacketFamily.Party;

        public override PacketAction Action => PacketAction.List;

        public PartyListHandler(IPlayerInfoProvider playerInfoProvider,
                                IPartyDataRepository partyDataRepository)
            : base(playerInfoProvider)
        {
            _partyDataRepository = partyDataRepository;
        }

        public override bool HandlePacket(PartyListServerPacket packet)
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

            return true;
        }
    }
}
