// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading;

namespace EOLib.Net.Connection
{
    public interface IBackgroundReceiveThreadRepository
    {
        Thread BackgroundThreadObject { get; set; }

        bool BackgroundThreadRunning { get; set; }
    }

    public class BackgroundReceiveThreadRepository : IBackgroundReceiveThreadRepository
    {
        public Thread BackgroundThreadObject { get; set; }

        public bool BackgroundThreadRunning { get; set; }
    }
}
