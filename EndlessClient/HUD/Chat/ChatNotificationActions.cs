using System;
using System.Globalization;
using System.Windows;
using AutomaticTypeMapper;
using EndlessClient.Audio;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Panels;
using EndlessClient.UIControls;
using EOLib;
using EOLib.Domain.Chat;
using EOLib.Domain.Notifiers;
using EOLib.Localization;
using Optional;

namespace EndlessClient.HUD.Chat
{
    [AutoMappedType]
    public class ChatNotificationActions : IChatEventNotifier
    {
        private readonly IChatRepository _chatRepository;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IServerMessageHandler _serverMessageHandler;
        private readonly ISfxPlayer _sfxPlayer;

        public ChatNotificationActions(IChatRepository chatRepository,
                                       IHudControlProvider hudControlProvider,
                                       ILocalizedStringFinder localizedStringFinder,
                                       IStatusLabelSetter statusLabelSetter,
                                       IServerMessageHandler serverMessageHandler,
                                       ISfxPlayer sfxPlayer)
        {
            _chatRepository = chatRepository;
            _hudControlProvider = hudControlProvider;
            _localizedStringFinder = localizedStringFinder;
            _statusLabelSetter = statusLabelSetter;
            _serverMessageHandler = serverMessageHandler;
            _sfxPlayer = sfxPlayer;
        }

        public void NotifyChatReceived(ChatEventType eventType)
        {
            _sfxPlayer.PlaySfx(eventType switch
            {
                ChatEventType.PrivateMessage => SoundEffectID.PrivateMessageReceived,
                ChatEventType.AdminChat => SoundEffectID.AdminChatReceived,
                ChatEventType.AdminAnnounce => SoundEffectID.AdminAnnounceReceived,
                ChatEventType.Group => SoundEffectID.GroupChatReceived,
                _ => SoundEffectID.LayeredTechIntro, // this will be funny if it ever gets hit
            });
        }

        public void NotifyPrivateMessageRecipientNotFound(string recipientName)
        {
            var whichTab = _chatRepository.PMTarget1.ToLower() == recipientName.ToLower()
                ? Option.Some(ChatTab.Private1)
                : _chatRepository.PMTarget2.ToLower() == recipientName.ToLower()
                    ? Option.Some(ChatTab.Private2)
                    : Option.None<ChatTab>();

            whichTab.MatchSome(tab =>
            {
                if (tab == ChatTab.Private1)
                    _chatRepository.PMTarget1 = null;
                else if (tab == ChatTab.Private2)
                    _chatRepository.PMTarget2 = null;

                _chatRepository.AllChat[tab].Clear();

                var chatPanel = _hudControlProvider.GetComponent<ChatPanel>(HudControlIdentifier.ChatPanel);
                chatPanel.ClosePMTab(tab);
            });

            _sfxPlayer.PlaySfx(SoundEffectID.PrivateMessageTargetNotFound);
        }

        public void NotifyPlayerMutedByAdmin(string adminName)
        {
            var chatTextBox = _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox);
            var chatMode = _hudControlProvider.GetComponent<ChatModePictureBox>(HudControlIdentifier.ChatModePictureBox);

            var endMuteTime = DateTime.Now.AddMinutes(Constants.MuteDefaultTimeMinutes);
            chatTextBox.SetMuted(endMuteTime);
            chatMode.SetMuted(endMuteTime);

            chatTextBox.Text = string.Empty;

            var chatData = new ChatData(ChatTab.Local, _localizedStringFinder.GetString(EOResourceID.STRING_SERVER),
                _localizedStringFinder.GetString(EOResourceID.CHAT_MESSAGE_MUTED_BY) + " " + adminName,
                ChatIcon.Exclamation,
                ChatColor.Server);
            _chatRepository.AllChat[ChatTab.Local].Add(chatData);

            _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_ACTION,
                Constants.MuteDefaultTimeMinutes.ToString(CultureInfo.InvariantCulture) + " ",
                EOResourceID.STATUS_LABEL_MINUTES_MUTED);
        }

        public void NotifyServerMessage(string serverMessage)
        {
            _serverMessageHandler.AddServerMessage(serverMessage);
        }

        public void NotifyServerPing(int timeInMS)
        {
            var message = $"[x] Current ping to the server is: {timeInMS} ms.";
            var chatData = new ChatData(ChatTab.Local, "System", message, ChatIcon.LookingDude);
            _chatRepository.AllChat[ChatTab.Local].Add(chatData);
        }
    }
}
