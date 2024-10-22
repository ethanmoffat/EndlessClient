using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Notifiers;
using EOLib.Localization;
using EOLib.Net.Communication;
using Moffat.EndlessOnline.SDK.Protocol.Net.Client;
using Moffat.EndlessOnline.SDK.Protocol.Net.Server;
using XNAControls;

namespace EndlessClient.Subscribers
{
    [AutoMappedType]
    public class GuildEventSubscriber : IGuildNotifier
    {
        private readonly IEOMessageBoxFactory _messageBoxFactory;
        private readonly IGuildActions _guildActions;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IPacketSendService _packetSendService;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IGuildSessionProvider _guildSessionProvider;

        public GuildEventSubscriber(IEOMessageBoxFactory messageBoxFactory,
            IGuildActions guildActions,
            ILocalizedStringFinder localizedStringFinder,
            IPacketSendService packetSendService,
            ISfxPlayer sfxPlayer,
            IGuildSessionProvider guildSessionProvider)
        {
            _messageBoxFactory = messageBoxFactory;
            _guildActions = guildActions;
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

        public void NotifyGuildReply(GuildReply reply)
        {
            var dialogMessage = reply switch
            {
                GuildReply.Updated => DialogResourceID.GUILD_DETAILS_UPDATED,
                GuildReply.NotFound => DialogResourceID.GUILD_DOES_NOT_EXIST,
                GuildReply.RecruiterOffline => DialogResourceID.GUILD_RECRUITER_NOT_FOUND,
                GuildReply.RecruiterNotHere => DialogResourceID.GUILD_RECRUITER_NOT_HERE,
                GuildReply.RecruiterWrongGuild => DialogResourceID.GUILD_RECRUITER_NOT_MEMBER,
                GuildReply.NotRecruiter => DialogResourceID.GUILD_RECRUITER_RANK_TOO_LOW,
                GuildReply.Busy => DialogResourceID.GUILD_MASTER_IS_BUSY,
                GuildReply.NotApproved => DialogResourceID.GUILD_CREATE_NAME_NOT_APPROVED,
                GuildReply.Exists => DialogResourceID.GUILD_TAG_OR_NAME_ALREADY_EXISTS,
                GuildReply.NoCandidates => DialogResourceID.GUILD_CREATE_NO_CANDIDATES,
                _ => default
            };

            if (dialogMessage == default)
                return;

            var dlg = _messageBoxFactory.CreateMessageBox(dialogMessage);
            dlg.ShowDialog();
        }

        public void NotifyConfirmCreateGuild()
        {
            _guildSessionProvider.CreationSession.MatchSome(creationSession =>
            {
                _sfxPlayer.PlaySfx(SoundEffectID.ServerMessage);

                var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_WILL_BE_CREATED, whichButtons: EODialogButtons.OkCancel);
                dlg.DialogClosing += (_, e) =>
                {
                    if (e.Result == XNADialogResult.OK)
                    {
                        _guildActions.ConfirmGuildCreate(creationSession);
                    }
                };
                dlg.Show();
            });
        }
    }
}
