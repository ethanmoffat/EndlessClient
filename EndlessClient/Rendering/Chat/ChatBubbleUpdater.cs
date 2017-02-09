// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EOLib;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Chat
{
    public class ChatBubbleUpdater : IChatBubbleUpdater
    {
        private readonly IChatBubbleRepository _chatBubbleRepository;

        public ChatBubbleUpdater(IChatBubbleRepository chatBubbleRepository)
        {
            _chatBubbleRepository = chatBubbleRepository;
        }

        public void UpdateChatBubbles(GameTime gameTime)
        {
            RemoveOldChatBubbles();

            if (_chatBubbleRepository.MainCharacterChatBubble.HasValue)
                _chatBubbleRepository.MainCharacterChatBubble.Value.Update();

            foreach (var chatBubble in _chatBubbleRepository.OtherCharacterChatBubbles.Values)
                chatBubble.Update();

            foreach (var chatBubble in _chatBubbleRepository.NPCChatBubbles.Values)
                chatBubble.Update();
        }

        private void RemoveOldChatBubbles()
        {
            if (_chatBubbleRepository.MainCharacterChatBubble.HasValue &&
                !_chatBubbleRepository.MainCharacterChatBubble.Value.ShowBubble)
            {
                _chatBubbleRepository.MainCharacterChatBubble.Value.Dispose();
                _chatBubbleRepository.MainCharacterChatBubble = Optional<IChatBubble>.Empty;
            }

            RemoveDoneChatBubbles(_chatBubbleRepository.OtherCharacterChatBubbles);
            RemoveDoneChatBubbles(_chatBubbleRepository.NPCChatBubbles);
        }

        private static void RemoveDoneChatBubbles(Dictionary<int, IChatBubble> mapping)
        {
            if (!mapping.Any())
                return;

            var done = new List<int>();
            foreach (var kvp in mapping.Where(pair => !pair.Value.ShowBubble))
            {
                kvp.Value.Dispose();
                done.Add(kvp.Key);
            }

            foreach (var id in done)
                mapping.Remove(id);
        }
    }

    public interface IChatBubbleUpdater
    {
        void UpdateChatBubbles(GameTime gameTime);
    }
}
