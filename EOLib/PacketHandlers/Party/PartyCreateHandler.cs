using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Party;
using EOLib.Net;
using EOLib.Net.Handlers;

namespace EOLib.PacketHandlers.Party
{
    /// <summary>
    /// Handles HP update for party members
    /// </summary>
    [AutoMappedType]
    public class PartyCreateHandler : InGameOnlyPacketHandler
    {
        private readonly IPartyDataRepository _partyDataRepository;

        public override PacketFamily Family => PacketFamily.Party;

        public override PacketAction Action => PacketAction.Create;

        public PartyCreateHandler(IPlayerInfoProvider playerInfoProvider,
                                  IPartyDataRepository partyDataRepository)
            : base(playerInfoProvider)
        {
            _partyDataRepository = partyDataRepository;
        }

        public override bool HandlePacket(IPacket packet)
        {
            _partyDataRepository.Members.Clear();

            while (packet.ReadPosition < packet.Length)
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
            }

            return true;
        }
    }
}
