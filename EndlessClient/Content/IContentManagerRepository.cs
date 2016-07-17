// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using Microsoft.Xna.Framework.Content;

namespace EndlessClient.Content
{
    public interface IContentManagerRepository
    {
        ContentManager Content { get; set; }
    }

    public interface IContentManagerProvider
    {
        ContentManager Content { get; }
    }

    public class ContentManagerRepository : IContentManagerRepository, IContentManagerProvider
    {
        public ContentManager Content { get; set; }
    }
}
