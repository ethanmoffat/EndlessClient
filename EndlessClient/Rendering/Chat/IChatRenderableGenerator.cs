using EOLib.Domain.Chat;
using System.Collections.Generic;

namespace EndlessClient.Rendering.Chat
{
    public interface IChatRenderableGenerator
    {
        IReadOnlyList<IChatRenderable> GenerateNewsRenderables(IReadOnlyList<string> newsText);

        IReadOnlyList<IChatRenderable> GenerateChatRenderables(IEnumerable<ChatData> chatData);
    }
}