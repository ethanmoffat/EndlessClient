// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;

namespace EndlessClient.Rendering.Chat
{
    public interface IChatRenderableGenerator
    {
        IReadOnlyList<IChatRenderable> GenerateNewsRenderables(IReadOnlyList<string> newsText);
    }
}
