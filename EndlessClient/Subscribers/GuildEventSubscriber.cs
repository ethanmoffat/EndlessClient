using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Dialogs;
using EndlessClient.Dialogs.Factories;
using EOLib.Domain.Interact.Guild;
using EOLib.Domain.Notifiers;
using EOLib.IO.Repositories;
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
        private readonly IPacketSendService _packetSendService;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly IGuildSessionProvider _guildSessionProvider;
        private readonly IEIFFileProvider _itemFileProvider;

        public GuildEventSubscriber(IEOMessageBoxFactory messageBoxFactory,
            IGuildActions guildActions,
            IPacketSendService packetSendService,
            ISfxPlayer sfxPlayer,
            IGuildSessionProvider guildSessionProvider,
            IEIFFileProvider itemFileProvider)
        {
            _messageBoxFactory = messageBoxFactory;
            _guildActions = guildActions;
            _packetSendService = packetSendService;
            _sfxPlayer = sfxPlayer;
            _guildSessionProvider = guildSessionProvider;
            _itemFileProvider = itemFileProvider;
        }

        public void NotifyGuildCreationRequest(int creatorPlayerID, string guildIdentity)
        {
            _sfxPlayer.PlaySfx(SoundEffectID.ServerMessage);

            var dlg = _messageBoxFactory.CreateMessageBox(
                prependData: $"{guildIdentity} ",
                resource: DialogResourceID.GUILD_INVITATION_INVITES_YOU,
                whichButtons: EODialogButtons.OkCancel,
                style: EOMessageBoxStyle.LargeDialogSmallHeader,
                EOResourceID.GUILD_JOINING_A_GUILD_IS_FREE,
                EOResourceID.GUILD_PLEASE_CONSIDER_CAREFULLY,
                EOResourceID.GUILD_DO_YOU_ACCEPT);

            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
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
                prependData: $"{name} ",
                resource: DialogResourceID.GUILD_PLAYER_WANTS_TO_JOIN,
                whichButtons: EODialogButtons.OkCancel,
                style: EOMessageBoxStyle.LargeDialogSmallHeader,
                EOResourceID.GUILD_YOUR_ACCOUNT_WILL_BE_CHARGED,
                EOResourceID.GUILD_PLEASE_CONSIDER_CAREFULLY_RECRUIT,
                EOResourceID.GUILD_DO_YOU_ACCEPT);

            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNADialogResult.OK)
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
                GuildReply.RemoveLeader => DialogResourceID.GUILD_REMOVE_PLAYER_IS_LEADER,
                GuildReply.RemoveNotMember => DialogResourceID.GUILD_REMOVE_PLAYER_NOT_MEMBER,
                GuildReply.Removed => DialogResourceID.GUILD_REMOVE_SUCCESS,
                _ => default
            };

            var prependData = reply switch
            {
                GuildReply.RemoveLeader or
                GuildReply.RemoveNotMember or
                GuildReply.Removed => $"{_guildSessionProvider.RemoveCandidate} ",
                _ => string.Empty
            };

            if (dialogMessage == default)
                return;

            var dlg = _messageBoxFactory.CreateMessageBox(prependData, dialogMessage);
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
                    else
                    {
                        _guildActions.CancelGuildCreate();
                    }
                };
                dlg.ShowDialog();
            });
        }

        public void NotifyNewGuildBankBalance(int balance)
        {
            _sfxPlayer.PlaySfx(SoundEffectID.BuySell);

            var goldName = _itemFileProvider.EIFFile[1].Name;
            var dlg = _messageBoxFactory.CreateMessageBox(DialogResourceID.GUILD_DEPOSIT_NEW_BALANCE, $" {balance} {goldName}");
            dlg.ShowDialog();
        }
    }
}
