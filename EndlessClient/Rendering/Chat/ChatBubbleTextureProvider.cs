using System.Collections.Generic;
using AutomaticTypeMapper;
using EndlessClient.Content;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Chat
{
    [MappedType(BaseType = typeof(IChatBubbleTextureProvider), IsSingleton = true)]
    public class ChatBubbleTextureProvider : IChatBubbleTextureProvider
    {
        private readonly IContentManagerProvider _contentManagerProvider;
        private readonly Dictionary<ChatBubbleTexture, Texture2D> _chatBubbleTextures;

        public IReadOnlyDictionary<ChatBubbleTexture, Texture2D> ChatBubbleTextures => _chatBubbleTextures;

        public ChatBubbleTextureProvider(IContentManagerProvider contentManagerProvider)
        {
            _contentManagerProvider = contentManagerProvider;
            _chatBubbleTextures = new Dictionary<ChatBubbleTexture, Texture2D>();
        }

        public void LoadContent()
        {
            _chatBubbleTextures.Add(ChatBubbleTexture.TopLeft, Content.Load<Texture2D>("ChatBubble\\TL"));
            _chatBubbleTextures.Add(ChatBubbleTexture.TopMiddle, Content.Load<Texture2D>("ChatBubble\\TM"));
            _chatBubbleTextures.Add(ChatBubbleTexture.TopRight, Content.Load<Texture2D>("ChatBubble\\TR"));
            _chatBubbleTextures.Add(ChatBubbleTexture.MiddleLeft, Content.Load<Texture2D>("ChatBubble\\ML"));
            _chatBubbleTextures.Add(ChatBubbleTexture.MiddleMiddle, Content.Load<Texture2D>("ChatBubble\\MM"));
            _chatBubbleTextures.Add(ChatBubbleTexture.MiddleRight, Content.Load<Texture2D>("ChatBubble\\MR"));
            //todo: change the first 'R' to a 'B' (for bottom)
            _chatBubbleTextures.Add(ChatBubbleTexture.BottomLeft, Content.Load<Texture2D>("ChatBubble\\RL"));
            _chatBubbleTextures.Add(ChatBubbleTexture.BottomMiddle, Content.Load<Texture2D>("ChatBubble\\RM"));
            _chatBubbleTextures.Add(ChatBubbleTexture.BottomRight, Content.Load<Texture2D>("ChatBubble\\RR"));
            _chatBubbleTextures.Add(ChatBubbleTexture.Nubbin, Content.Load<Texture2D>("ChatBubble\\NUB"));
        }

        private ContentManager Content => _contentManagerProvider.Content;
    }

    public interface IChatBubbleTextureProvider
    {
        IReadOnlyDictionary<ChatBubbleTexture, Texture2D> ChatBubbleTextures { get; }

        void LoadContent();
    }
}
