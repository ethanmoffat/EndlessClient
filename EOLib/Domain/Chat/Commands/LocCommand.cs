// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EOLib.Domain.Character;
using EOLib.Localization;

namespace EOLib.Domain.Chat.Commands
{
    public class LocCommand : IPlayerCommand
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly IChatRepository _chatRepository;
        private readonly ILocalizedStringService _localizedStringService;

        public string CommandText { get { return "loc"; } }

        public LocCommand(ICharacterProvider characterProvider,
            IChatRepository chatRepository,
            ILocalizedStringService localizedStringService)
        {
            _characterProvider = characterProvider;
            _chatRepository = chatRepository;
            _localizedStringService = localizedStringService;
        }

        public bool Execute(string parameter)
        {
            var firstPart = _localizedStringService.GetString(EOResourceID.STATUS_LABEL_YOUR_LOCATION_IS_AT);
            var message = string.Format(firstPart + " {0}  x:{1}  y:{2}",
                _characterProvider.MainCharacter.MapID,
                _characterProvider.MainCharacter.RenderProperties.MapX,
                _characterProvider.MainCharacter.RenderProperties.MapY);

            var chatData = new ChatData("System", message, ChatIcon.LookingDude);
            _chatRepository.AllChat[ChatTab.Local].Add(chatData);

            return true;
        }
    }
}