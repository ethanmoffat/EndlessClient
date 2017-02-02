// Original Work Copyright (c) Ethan Moffat 2014-2017
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EndlessClient.Content;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.Chat
{
    public class ChatBubbleTextureProvider : IChatBubbleTextureProvider
    {
        private readonly IContentManagerProvider _contentManagerProvider;
        private readonly Dictionary<ChatBubbleTexture, Texture2D> _chatBubbleTextures;

        public IReadOnlyDictionary<ChatBubbleTexture, Texture2D> ChatBubbleTextures => _chatBubbleTextures;

        public ChatBubbleTextureProvider(IContentManagerProvider contentManagerProvider)
        {
            _contentManagerProvider = contentManagerProvider;

            _chatBubbleTextures = new Dictionary<ChatBubbleTexture, Texture2D>
            {
                {ChatBubbleTexture.TopLeft, Content.Load<Texture2D>("ChatBubble\\TL")},
                {ChatBubbleTexture.TopMiddle, Content.Load<Texture2D>("ChatBubble\\TM")},
                {ChatBubbleTexture.TopRight, Content.Load<Texture2D>("ChatBubble\\TR")},
                {ChatBubbleTexture.MiddleLeft, Content.Load<Texture2D>("ChatBubble\\ML")},
                {ChatBubbleTexture.MiddleMiddle, Content.Load<Texture2D>("ChatBubble\\MM")},
                {ChatBubbleTexture.MiddleRight, Content.Load<Texture2D>("ChatBubble\\MR")},
                //todo: change the first 'R' to a 'B' (for bottom)
                {ChatBubbleTexture.BottomLeft, Content.Load<Texture2D>("ChatBubble\\RL")},
                {ChatBubbleTexture.BottomMiddle, Content.Load<Texture2D>("ChatBubble\\RM")},
                {ChatBubbleTexture.BottomRight, Content.Load<Texture2D>("ChatBubble\\RR")}
            };
        }

        private ContentManager Content => _contentManagerProvider.Content;
    }

    public interface IChatBubbleTextureProvider
    {
        IReadOnlyDictionary<ChatBubbleTexture, Texture2D> ChatBubbleTextures { get; }
    }
}
