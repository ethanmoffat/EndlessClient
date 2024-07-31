using AutomaticTypeMapper;
using EndlessClient.Audio;
using EOLib.Domain.Chat;
using EOLib.Localization;

namespace EndlessClient.HUD.Chat
{
    [AutoMappedType]
    public class ServerMessageHandler : IServerMessageHandler
    {
        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringFinder _localizedStringFinder;
        private readonly ISfxPlayer _sfxPlayer;

        public ServerMessageHandler(IChatRepository chatRepository,
                                    ILocalizedStringFinder localizedStringFinder,
                                    ISfxPlayer sfxPlayer)
        {
            _chatRepository = chatRepository;
            _localizedStringFinder = localizedStringFinder;
            _sfxPlayer = sfxPlayer;
        }

        public void AddServerMessage(string serverMessage, SoundEffectID soundEffect = SoundEffectID.Login, ChatIcon icon = ChatIcon.Exclamation)
        {
            var server = _localizedStringFinder.GetString(EOResourceID.STRING_SERVER);

            var localData = new ChatData(ChatTab.Local, server, serverMessage, icon, ChatColor.Server, log: false, filter: false);
            var globalData = new ChatData(ChatTab.Global, server, serverMessage, icon, ChatColor.ServerGlobal, log: false, filter: false);
            var systemData = new ChatData(ChatTab.System, string.Empty, serverMessage, icon, ChatColor.Server, filter: false);

            _chatRepository.AllChat[ChatTab.Local].Add(localData);
            _chatRepository.AllChat[ChatTab.Global].Add(globalData);
            _chatRepository.AllChat[ChatTab.System].Add(systemData);

            _sfxPlayer.PlaySfx(soundEffect);
        }
    }

    public interface IServerMessageHandler
    {
        void AddServerMessage(string message, SoundEffectID soundEffect = SoundEffectID.ServerMessage, ChatIcon icon = ChatIcon.Exclamation);
    }
}