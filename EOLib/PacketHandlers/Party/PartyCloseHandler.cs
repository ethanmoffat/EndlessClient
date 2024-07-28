using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Party;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Party
{
    /// <summary>
    /// Handles leaving (or being removed from) a party
    /// </summary>
    [AutoMappedType]
    public class PartyCloseHandler : InGameOnlyPacketHandler<PartyCloseServerPacket>
    {
        private readonly IPartyDataRepository _partyDataRepository;

        public override PacketFamily Family => PacketFamily.Party;

        public override PacketAction Action => PacketAction.Close;

        public PartyCloseHandler(IPlayerInfoProvider playerInfoProvider,
                                 IPartyDataRepository partyDataRepository)
            : base(playerInfoProvider)
        {
            _partyDataRepository = partyDataRepository;
        }

        public override bool HandlePacket(PartyCloseServerPacket packet)
        {
            _partyDataRepository.Members.Clear();
            return true;
        }
    }
}