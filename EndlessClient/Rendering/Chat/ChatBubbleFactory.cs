using AutomaticTypeMapper;
using EndlessClient.GameExecution;
using EOLib.Config;

namespace EndlessClient.Rendering.Chat
{
    [AutoMappedType]
    public class ChatBubbleFactory : IChatBubbleFactory
    {
        private readonly IChatBubbleTextureProvider _chatBubbleTextureProvider;
        private readonly IEndlessGameProvider _endlessGameProvider;
        private readonly IConfigurationProvider _configurationProvider;

        public ChatBubbleFactory(IChatBubbleTextureProvider chatBubbleTextureProvider,
                                 IEndlessGameProvider endlessGameProvider,
                                 IConfigurationProvider configurationProvider)
        {
            _chatBubbleTextureProvider = chatBubbleTextureProvider;
            _endlessGameProvider = endlessGameProvider;
            _configurationProvider = configurationProvider;
        }

        public IChatBubble CreateChatBubble(IMapActor owner)
        {
            var chatBubble = new ChatBubble(owner, _chatBubbleTextureProvider, _endlessGameProvider, _configurationProvider);

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