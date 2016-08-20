// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using EOLib.Domain.Character;

namespace EOLib.Domain.Chat.Commands
{
    public class UsageCommand : IPlayerCommand
    {
        private readonly ICharacterProvider _characterProvider;
        private readonly IChatRepository _chatRepository;

        public string CommandText { get { return "usage"; } }

        public UsageCommand(ICharacterProvider characterProvider,
                            IChatRepository chatRepository)
        {
            _characterProvider = characterProvider;
            _chatRepository = chatRepository;
        }

        public bool Execute(string parameter)
        {
            var usage = _characterProvider.MainCharacter.Stats[CharacterStat.Usage];
            var message = string.Format("[x] usage: {0}hrs. {1}min.", usage/60, usage%60);

            var chatData = new ChatData("System", message, ChatIcon.LookingDude);
            _chatRepository.AllChat[ChatTab.Local].Add(chatData);

            return true;
        }
    }
}
