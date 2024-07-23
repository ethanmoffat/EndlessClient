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

        public GuildEventSubscriber(IEOMessageBoxFactory messageBoxFactory, ILocalizedStringFinder localizedStringFinder, IPacketSendService packetSendService, ISfxPlayer sfxPlayer)
        {
            _messageBoxFactory = messageBoxFactory;
            _localizedStringFinder = localizedStringFinder;
            _packetSendService = packetSendService;
            _sfxPlayer = sfxPlayer;
        }

        public void NotifyGuildCreationRequest(GuildCreationRequest request)
        {
            _sfxPlayer.PlaySfx(SoundEffectID.ServerMessage);

            var dlg = _messageBoxFactory.CreateMessageBox(
                $"{request.GuildIdentity}" + " " +
                _localizedStringFinder.GetString(DialogResourceID.GUILD_INVITES_YOU_TO_JOIN) + " " +
                _localizedStringFinder.GetString(EOResourceID.GUILD_JOINING_A_GUILD_IS_FREE) + " " +
                _localizedStringFinder.GetString(EOResourceID.GUILD_PLEASE_CONSIDER_CAREFULLY) + " " +
                _localizedStringFinder.GetString(EOResourceID.GUILD_DO_YOU_ACCEPT)
                , caption: _localizedStringFinder.GetString(DialogResourceID.GUILD_INVITATION), whichButtons: Dialogs.EODialogButtons.OkCancel,
                style: Dialogs.EOMessageBoxStyle.LargeDialogSmallHeader);

            dlg.DialogClosing += (_, e) =>
            {
                if (e.Result == XNAControls.XNADialogResult.OK)
                {
                    var packet = new GuildAcceptClientPacket();
                    packet.InviterPlayerId = request.CreatorPlayerID;
                    _packetSendService.SendPacket(packet);
                }
            };

            dlg.ShowDialog();
        }
    }
}
