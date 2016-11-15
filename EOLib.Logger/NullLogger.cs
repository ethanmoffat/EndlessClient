// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Logger
{
    public sealed class NullLogger : ILogger
    {
        public void Dispose() { }

        public void Log(string format, params object[] parameters) { }
    }
}
