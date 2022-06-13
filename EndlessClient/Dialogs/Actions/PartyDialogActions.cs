using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
using EndlessClient.HUD;
using EOLib.Domain.Chat;
using EOLib.Domain.Notifiers;
using EOLib.Domain.Party;
using EOLib.Localization;
using XNAControls;

namespace EndlessClient.Dialogs.Actions
{
    [AutoMappedType]
    public class PartyDialogActions : IPartyEventNotifier
    {
        private readonly IPartyActions _partyActions;
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IChatRepository _chatRepository;

        public PartyDialogActions(IPartyActions partyActions,
                                  IEOMessageBoxFactory messageBoxFactory,
                                  IStatusLabelSetter statusLabelSetter,
                                  ILocalizedStringFinder localizedStringFinder,
                                  IChatRepository chatRepository)
        {
            _partyActions = partyActions;
            _messageBoxFactory = messageBoxFactory;
            _statusLabelSetter = statusLabelSetter;
            _localizedStringFinder = localizedStringFinder;
            _chatRepository = chatRepository;
        }

        public void NotifyPartyRequest(PartyRequestType type, short playerId, string name)
        {
            name = char.ToUpper(name[0]) + name[1..];
            var dlg = _messageBoxFactory.CreateMessageBox(name,
                type == PartyRequestType.Join
                    ? DialogResourceID.PARTY_GROUP_REQUEST_TO_JOIN
                    : DialogResourceID.PARTY_GROUP_SEND_INVITATION,
                EODialogButtons.OkCancel);
            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    _partyActions.AcceptParty(type, playerId);

                    var message = type == PartyRequestType.Join
                        ? EOResourceID.STATUS_LABEL_PARTY_YOU_JOINED
                        : EOResourceID.STATUS_LABEL_PARTY_JOINED_YOUR;
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, name, message);
                }
            };
            dlg.ShowDialog();
        }

        public void NotifyPartyMemberAdd(string name)
        {
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, name, EOResourceID.STATUS_LABEL_PARTY_JOINED_YOUR);
            _chatRepository.AllChat[ChatTab.System].Add(new ChatData(ChatTab.System, string.Empty, _localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_PARTY_JOINED_YOUR), ChatIcon.PlayerParty, ChatColor.PM));
        }

        public void NotifyPartyMemberRemove(string name)
        {
            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, name, EOResourceID.STATUS_LABEL_PARTY_LEFT_YOUR);
            _chatRepository.AllChat[ChatTab.System].Add(new ChatData(ChatTab.System, string.Empty, _localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_PARTY_LEFT_YOUR), ChatIcon.PlayerPartyDark, ChatColor.PM));
        }
    }
}
