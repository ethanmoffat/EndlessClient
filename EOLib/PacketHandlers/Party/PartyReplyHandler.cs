using System.Collections.Generic;
using AutomaticTypeMapper;
using EOLib.Domain.Interact.Quest;
using EOLib.Domain.Login;
using EOLib.Localization;
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
        private readonly IEnumerable<IStatusLabelNotifier> _statusLabelNotifiers;
        private readonly ILocalizedStringFinder _localizedStringFinder;

        public override PacketFamily Family => PacketFamily.Party;

        public override PacketAction Action => PacketAction.Reply;

        public PartyReplyHandler(IPlayerInfoProvider playerInfoProvider,
                                   IEnumerable<IStatusLabelNotifier> statusLabelNotifiers,
                                   ILocalizedStringFinder localizedStringFinder)
            : base(playerInfoProvider)
        {
            _statusLabelNotifiers = statusLabelNotifiers;
            _localizedStringFinder = localizedStringFinder;
        }

        public override bool HandlePacket(PartyReplyServerPacket packet)
        {
            foreach (var notifier in _statusLabelNotifiers)
            {
                switch (packet.ReplyCode)
                {
                    case PartyReplyCode.PartyIsFull:
                        notifier.ShowWarning(_localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_PARTY_THE_PARTY_IS_FULL));
                        break;
                    case PartyReplyCode.AlreadyInAnotherParty:
                        var anotherPartyData = (PartyReplyServerPacket.ReplyCodeDataAlreadyInAnotherParty)packet.ReplyCodeData;
                        notifier.ShowWarning($"${anotherPartyData.PlayerName} ${_localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_PARTY_IS_ALREADY_IN_ANOTHER_PARTY)}");
                        break;
                    case PartyReplyCode.AlreadyInYourParty:
                        var yourPartyData = (PartyReplyServerPacket.ReplyCodeDataAlreadyInYourParty)packet.ReplyCodeData;
                        notifier.ShowWarning($"${yourPartyData.PlayerName} ${_localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_PARTY_IS_ALREADY_MEMBER)}");
                        break;
                }
            }

            return true;
        }
    }
}
