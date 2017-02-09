// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib;

namespace EndlessClient.Rendering.Chat
{
    public interface IChatBubbleRepository
    {
        Optional<IChatBubble> MainCharacterChatBubble { get; set; }

        Dictionary<int, IChatBubble> OtherCharacterChatBubbles { get; set; }
    }

    public interface IChatBubbleProvider
    {
        Optional<IChatBubble> MainCharacterChatBubble { get; }

        IReadOnlyDictionary<int, IChatBubble> OtherCharacterChatBubbles { get; }
    }

    public class ChatBubbleRepository : IChatBubbleRepository, IChatBubbleProvider
    {
        public Optional<IChatBubble> MainCharacterChatBubble { get; set; }

        public Dictionary<int, IChatBubble> OtherCharacterChatBubbles { get; set; }

        IReadOnlyDictionary<int, IChatBubble> IChatBubbleProvider.OtherCharacterChatBubbles => OtherCharacterChatBubbles;

        public ChatBubbleRepository()
        {
            MainCharacterChatBubble = Optional<IChatBubble>.Empty;
            OtherCharacterChatBubbles = new Dictionary<int, IChatBubble>();
        }
    }
}
