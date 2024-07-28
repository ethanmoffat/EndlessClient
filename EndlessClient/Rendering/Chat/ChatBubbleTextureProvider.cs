using AutomaticTypeMapper;
using EndlessClient.Content;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace EndlessClient.Rendering.Chat;

[MappedType(BaseType = typeof(IChatBubbleTextureProvider), IsSingleton = true)]
public class ChatBubbleTextureProvider : IChatBubbleTextureProvider
{
    private readonly IContentProvider _contentProvider;
    private readonly Dictionary<ChatBubbleTexture, Texture2D> _chatBubbleTextures;

    public IReadOnlyDictionary<ChatBubbleTexture, Texture2D> ChatBubbleTextures => _chatBubbleTextures;

    public ChatBubbleTextureProvider(IContentProvider contentProvider)
    {
        _contentProvider = contentProvider;
        _chatBubbleTextures = new Dictionary<ChatBubbleTexture, Texture2D>();
    }

    public void LoadContent()
    {
        _chatBubbleTextures.Add(ChatBubbleTexture.TopLeft, _contentProvider.Textures[ContentProvider.ChatTL]);
        _chatBubbleTextures.Add(ChatBubbleTexture.TopMiddle, _contentProvider.Textures[ContentProvider.ChatTM]);
        _chatBubbleTextures.Add(ChatBubbleTexture.TopRight, _contentProvider.Textures[ContentProvider.ChatTR]);
        _chatBubbleTextures.Add(ChatBubbleTexture.MiddleLeft, _contentProvider.Textures[ContentProvider.ChatML]);
        _chatBubbleTextures.Add(ChatBubbleTexture.MiddleMiddle, _contentProvider.Textures[ContentProvider.ChatMM]);
        _chatBubbleTextures.Add(ChatBubbleTexture.MiddleRight, _contentProvider.Textures[ContentProvider.ChatMR]);
        //todo: change the first 'R' to a 'B' (for bottom)
        _chatBubbleTextures.Add(ChatBubbleTexture.BottomLeft, _contentProvider.Textures[ContentProvider.ChatRL]);
        _chatBubbleTextures.Add(ChatBubbleTexture.BottomMiddle, _contentProvider.Textures[ContentProvider.ChatRM]);
        _chatBubbleTextures.Add(ChatBubbleTexture.BottomRight, _contentProvider.Textures[ContentProvider.ChatRR]);
        _chatBubbleTextures.Add(ChatBubbleTexture.Nubbin, _contentProvider.Textures[ContentProvider.ChatNUB]);
    }
}

public interface IChatBubbleTextureProvider
{
    IReadOnlyDictionary<ChatBubbleTexture, Texture2D> ChatBubbleTextures { get; }

    void LoadContent();
}