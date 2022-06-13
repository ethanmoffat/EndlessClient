using AutomaticTypeMapper;
using EndlessClient.Dialogs.Factories;
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

        public PartyDialogActions(IPartyActions partyActions,
                                  IEOMessageBoxFactory messageBoxFactory)
        {
            _partyActions = partyActions;
            _messageBoxFactory = messageBoxFactory;
        }

        public void NotifyPartyRequest(PartyRequestType type, short playerId, string name)
        {
            var dlg = _messageBoxFactory.CreateMessageBox(
                char.ToUpper(name[0]) + name[1..],
                type == PartyRequestType.Join ? DialogResourceID.PARTY_GROUP_REQUEST_TO_JOIN : DialogResourceID.PARTY_GROUP_SEND_INVITATION,
                EODialogButtons.OkCancel);
            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
                {
                    _partyActions.AcceptParty(type, playerId);
                }
            };
            dlg.ShowDialog();
        }
    }
}
