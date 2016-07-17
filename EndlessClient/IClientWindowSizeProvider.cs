// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EndlessClient
{
    public interface IClientWindowSizeProvider
    {
        int Width { get; }
        int Height { get; }
    }

    public class ClientWindowSizeProvider : IClientWindowSizeProvider
    {
        //This could be extended to support adjusting the window size
        //Controls would need to use relative positioning instead of their current absolute coordinates
        //They would also need to support updating layout when the window is resized

        //Supporting dynamic window sizing is NOT a trivial task
        public int Width { get { return 640; } }
        public int Height { get { return 480; } }
    }
}
