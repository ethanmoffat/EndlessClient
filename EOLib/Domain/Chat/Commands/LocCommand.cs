using AutomaticTypeMapper;
using EOLib.Domain.Character;
using EOLib.Localization;

namespace EOLib.Domain.Chat.Commands
{
    [AutoMappedType]
    public class LocCommand : IPlayerCommand
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringFinder _localizedStringFinder;

        public string CommandText => "loc";

        public LocCommand(ICharacterProvider characterProvider,
            IChatRepository chatRepository,
            ILocalizedStringFinder localizedStringFinder)
        {
            _characterProvider = characterProvider;
            _chatRepository = chatRepository;
            _localizedStringFinder = localizedStringFinder;
        }

        public bool Execute(string parameter)
        {
            var firstPart = _localizedStringFinder.GetString(EOResourceID.STATUS_LABEL_YOUR_LOCATION_IS_AT);
            var message = string.Format(firstPart + " {0}  x:{1}  y:{2}",
                _characterProvider.MainCharacter.MapID,
                _characterProvider.MainCharacter.RenderProperties.MapX,
                _characterProvider.MainCharacter.RenderProperties.MapY);

            var chatData = new ChatData(ChatTab.Local, "System", message, ChatIcon.LookingDude);
            _chatRepository.AllChat[ChatTab.Local].Add(chatData);

            return true;
        }
    }
}
