using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.Dialogs.Factories;
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

        public GuildEventSubscriber(IEOMessageBoxFactory messageBoxFactory, ILocalizedStringFinder localizedStringFinder, IPacketSendService packetSendService, ISfxPlayer sfxPlayer)
        {
            _messageBoxFactory = messageBoxFactory;
            _localizedStringFinder = localizedStringFinder;
            _packetSendService = packetSendService;
            _sfxPlayer = sfxPlayer;
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
    }
}
