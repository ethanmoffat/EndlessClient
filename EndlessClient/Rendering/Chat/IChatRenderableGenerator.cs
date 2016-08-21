// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using EOLib.Domain.Chat;

namespace EndlessClient.Rendering.Chat
{
    public interface IChatRenderableGenerator
    {
        IReadOnlyList<IChatRenderable> GenerateNewsRenderables(IReadOnlyList<string> newsText);

        IReadOnlyList<IChatRenderable> GenerateChatRenderables(IReadOnlyList<ChatData> chatData);
    }
}
