// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Chat;
using EOLib.Domain.Chat;

namespace EndlessClient.HUD.Chat
{
    public class ChatBubbleActions : IChatBubbleActions
    {
        private readonly IChatBubbleRepository _chatBubbleRepository;
        private readonly IChatProcessor _chatProcessor;
        private readonly IChatTypeCalculator _chatTypeCalculator;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IChatBubbleTextureProvider _chatBubbleTextureProvider;

        public ChatBubbleActions(IChatBubbleRepository chatBubbleRepository,
                                 IChatProcessor chatProcessor,
                                 IChatTypeCalculator chatTypeCalculator,
                                 ICharacterRendererProvider characterRendererProvider,
                                 IChatBubbleTextureProvider chatBubbleTextureProvider)
        {
            _chatBubbleRepository = chatBubbleRepository;
            _chatProcessor = chatProcessor;
            _chatTypeCalculator = chatTypeCalculator;
            _characterRendererProvider = characterRendererProvider;
            _chatBubbleTextureProvider = chatBubbleTextureProvider;
        }

        public void ShowChatBubbleForMainCharacter(string input)
        {
            //todo: don't show chat bubble if group chat and character is not in a group (party)

            var chatType = _chatTypeCalculator.CalculateChatType(input);
            if (chatType != ChatType.Local &&
                chatType != ChatType.Party &&
                chatType != ChatType.Announce)
                return;

            var text = _chatProcessor.RemoveFirstCharacterIfNeeded(input, chatType, string.Empty);

            if (_chatBubbleRepository.MainCharacterChatBubble.HasValue)
                _chatBubbleRepository.MainCharacterChatBubble.Value.SetMessage(text, chatType == ChatType.Party);
            else
            {
                var chatBubble = new ChatBubble(text,
                                                _characterRendererProvider.MainCharacterRenderer,
                                                _chatBubbleTextureProvider);
                _chatBubbleRepository.MainCharacterChatBubble = chatBubble;
            }
        }
    }

    public interface IChatBubbleActions
    {
        void ShowChatBubbleForMainCharacter(string input);
    }
}