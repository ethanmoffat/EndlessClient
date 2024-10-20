using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Notifiers;
using EOLib.Localization;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;

namespace EndlessClient.Subscribers
{
    [AutoMappedType]
    public class GuildEventSubscriber : IGuildNotifier
    {
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IPacketSendService _packetSendService;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IGuildSessionProvider _guildSessionProvider;

        public GuildEventSubscriber(IEOMessageBoxFactory messageBoxFactory,
            ILocalizedStringFinder localizedStringFinder,
            IPacketSendService packetSendService,
            ISfxPlayer sfxPlayer,
            IGuildSessionProvider guildSessionProvider)
        {
            _messageBoxFactory = messageBoxFactory;
            _localizedStringFinder = localizedStringFinder;
            _packetSendService = packetSendService;
            _sfxPlayer = sfxPlayer;
            _guildSessionProvider = guildSessionProvider;
        }

        public void NotifyGuildCreationRequest(int creatorPlayerID, string guildIdentity)
        {
            _sfxPlayer.PlaySfx(SoundEffectID.ServerMessage);

            var dlg = _messageBoxFactory.CreateMessageBox(
                $"{guildIdentity}" +
                " " + _localizedStringFinder.GetString(DialogResourceID.GUILD_INVITES_YOU_TO_JOIN) +
                " " + _localizedStringFinder.GetString(EOResourceID.GUILD_JOINING_A_GUILD_IS_FREE) +
                " " + _localizedStringFinder.GetString(EOResourceID.GUILD_PLEASE_CONSIDER_CAREFULLY) +
                " " + _localizedStringFinder.GetString(EOResourceID.GUILD_DO_YOU_ACCEPT),
                caption: _localizedStringFinder.GetString(DialogResourceID.GUILD_INVITATION),
                whichButtons: Dialogs.EODialogButtons.OkCancel,
                style: Dialogs.EOMessageBoxStyle.LargeDialogSmallHeader);

            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNAControls.XNADialogResult.OK)
                {
                    _packetSendService.SendPacket(new GuildAcceptClientPacket()
                    {
                        InviterPlayerId = creatorPlayerID
                    });
                }
            };

            dlg.ShowDialog();
        }

        public void NotifyRequestToJoinGuild(int playerId, string name)
        {
            _sfxPlayer.PlaySfx(SoundEffectID.ServerMessage);

            var dlg = _messageBoxFactory.CreateMessageBox(
                $"{name}" +
                " " + _localizedStringFinder.GetString(DialogResourceID.GUILD_REQUESTED_TO_JOIN) +
                " " + _localizedStringFinder.GetString(EOResourceID.GUILD_YOUR_ACCOUNT_WILL_BE_CHARGED) +
                " " + _localizedStringFinder.GetString(EOResourceID.GUILD_PLEASE_CONSIDER_CAREFULLY_RECRUIT) +
                " " + _localizedStringFinder.GetString(EOResourceID.GUILD_DO_YOU_ACCEPT),
                caption: _localizedStringFinder.GetString(DialogResourceID.GUILD_PLAYER_WANTS_TO_JOIN),
                whichButtons: Dialogs.EODialogButtons.OkCancel,
                style: Dialogs.EOMessageBoxStyle.LargeDialogSmallHeader);

            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNAControls.XNADialogResult.OK)
                {
                    _packetSendService.SendPacket(new GuildUseClientPacket()
                    {
                        PlayerId = playerId,
                    });
                }
            };

            dlg.ShowDialog();
        }

        public void NotifyGuildDetailsUpdated()
        {
            var dlg = _messageBoxFactory.CreateMessageBox(_localizedStringFinder.GetString(DialogResourceID.GUILD_DETAILS_HAVE_BEEN_UPDATED),
                caption: _localizedStringFinder.GetString(DialogResourceID.GUILD_ACCEPTED),
                whichButtons: Dialogs.EODialogButtons.Ok,
                style: Dialogs.EOMessageBoxStyle.SmallDialogSmallHeader);

            dlg.ShowDialog();
        }

        public void NotifyRecruiterOffline()
        {
            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_RECRUITER_NOT_FOUND);
            dlg.Show();
        }

        public void NotifyRecruiterNotHere()
        {
            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_RECRUITER_NOT_HERE);
            dlg.Show();
        }

        public void NotifyRecruiterWrongGuild()
        {
            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_RECRUITER_NOT_MEMBER);
            dlg.Show();
        }

        public void NotifyNotRecruiter()
        {
            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_RECRUITER_RANK_TOO_LOW);
            dlg.Show();
        }

        public void NotifyBusy()
        {
            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_MASTER_IS_BUSY);
            dlg.Show();
        }

        public void NotifyConfirmCreateGuild()
        {
            _sfxPlayer.PlaySfx(SoundEffectID.ServerMessage);
            _guildSessionProvider.CreationSession.MatchSome(creationSession =>
            {
                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_WILL_BE_CREATED, whichButtons: Dialogs.EODialogButtons.OkCancel);
                dlg.DialogClosing += (_, e) =>
                {
                    if (e.Result == XNAControls.XNADialogResult.OK)
                    {
                        _packetSendService.SendPacket(new GuildCreateClientPacket()
                        {
                            SessionId = _guildSessionProvider.SessionID,
                            GuildTag = creationSession.Tag,
                            GuildName = creationSession.Name,
                            Description = creationSession.Description,
                        });
                    }
                };
                dlg.Show();
            });
        }

        public void NotifyNotApproved()
        {
            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_NAME_NOT_APPROVED);
            dlg.Show();
        }

        public void NotifyExists()
        {
            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_TAG_OR_NAME_ALREADY_EXISTS);
            dlg.Show();
        }

        public void NotifyNoCandidates()
        {
            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_CREATE_NO_CANDIDATES);
            dlg.Show();
        }
    }
}
