using AutomaticTypeMapper;
using EndlessClient.GameExecution;

namespace EndlessClient.Rendering.Chat
{
    [AutoMappedType]
    public class ChatBubbleFactory : IChatBubbleFactory
    {
        private readonly IChatBubbleTextureProvider _chatBubbleTextureProvider;
        private readonly IEndlessGameProvider _endlessGameProvider;

        public ChatBubbleFactory(IChatBubbleTextureProvider chatBubbleTextureProvider,
                                 IEndlessGameProvider endlessGameProvider)
        {
            _chatBubbleTextureProvider = chatBubbleTextureProvider;
            _endlessGameProvider = endlessGameProvider;
        }

        public IChatBubble CreateChatBubble(IMapActor owner)
        {
            var chatBubble = new ChatBubble(owner, _chatBubbleTextureProvider, _endlessGameProvider);

            if (!_endlessGameProvider.Game.Components.Contains(chatBubble))
                _endlessGameProvider.Game.Components.Add(chatBubble);

            return chatBubble;
        }
    }

    public interface IChatBubbleFactory
    {
        IChatBubble CreateChatBubble(IMapActor owner);
    }
}
