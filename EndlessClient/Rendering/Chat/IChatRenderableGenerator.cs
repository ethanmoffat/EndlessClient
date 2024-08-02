using System.Collections.Generic;
using EOLib.Domain.Chat;

namespace EndlessClient.Rendering.Chat
{
    public interface IChatRenderableGenerator
    {
        IReadOnlyList<IChatRenderable> GenerateNewsRenderables(IReadOnlyList<string> newsText);

        IReadOnlyList<IChatRenderable> GenerateChatRenderables(IEnumerable<ChatData> chatData);
    }
}