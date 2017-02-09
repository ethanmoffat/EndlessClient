// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Chat
{
    public class ChatBubbleUpdater : IChatBubbleUpdater
    {
        private readonly IChatBubbleProvider _chatBubbleProvider;

        public ChatBubbleUpdater(IChatBubbleProvider chatBubbleProvider)
        {
            _chatBubbleProvider = chatBubbleProvider;
        }

        public void UpdateChatBubbles(GameTime gameTime)
        {
            if (_chatBubbleProvider.MainCharacterChatBubble.HasValue)
                _chatBubbleProvider.MainCharacterChatBubble.Value.Update();

            foreach (var chatBubble in _chatBubbleProvider.OtherCharacterChatBubbles.Values)
                chatBubble.Update();

            foreach (var chatBubble in _chatBubbleProvider.NPCChatBubbles.Values)
                chatBubble.Update();
        }
    }

    public interface IChatBubbleUpdater
    {
        void UpdateChatBubbles(GameTime gameTime);
    }
}
