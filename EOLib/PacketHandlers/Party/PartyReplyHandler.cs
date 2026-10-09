using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Login;
using EOLib.Domain.Notifiers;
using EOLib.Net.Handlers;
using Moffat.EndlessOnline.SDK.Protocol.Net;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;

namespace EOLib.PacketHandlers.Party
{
    /// <summary>
    /// Handles party request failures
    /// </summary>
    [AutoMappedType]
    public class PartyReplyHandler : InGameOnlyPacketHandler<PartyReplyServerPacket>
    {
        private readonly IEnumerable<IPartyEventNotifier> _partyEventNotifiers;

        public override PacketFamily Family => PacketFamily.Party;

        public override PacketAction Action => PacketAction.Reply;

        public PartyReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                   IEnumerable<IPartyEventNotifier> partyEventNotifiers) : base(playerInfoProvider)
        {
            _partyEventNotifiers = partyEventNotifiers;
        }

        public override bool HandlePacket(PartyReplyServerPacket packet)
        {
            foreach (var notifier in _partyEventNotifiers)
            {
                switch (packet.ReplyCode)
                {
                    case PartyReplyCode.PartyIsFull:
                        notifier.NotifyPartyFull();
                        break;
                    case PartyReplyCode.AlreadyInAnotherParty:
                        var anotherPartyData = (PartyReplyServerPacket.ReplyCodeDataAlreadyInAnotherParty)packet.ReplyCodeData;
                        notifier.NotifyAlreadyInAnotherParty(anotherPartyData.PlayerName);
                        break;
                    case PartyReplyCode.AlreadyInYourParty:
                        var yourPartyData = (PartyReplyServerPacket.ReplyCodeDataAlreadyInYourParty)packet.ReplyCodeData;
                        notifier.NotifyAlreadyInYourParty(yourPartyData.PlayerName);
                        break;
                }
            }

            return true;
        }
    }
}
