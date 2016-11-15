// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

namespace EOLib.Logger
{
    internal static class Constants
    {
        internal const int FLUSH_TIME_MS = 30000;
        internal const int FLUSH_LINES_WRITTEN = 50;
        internal const int SPLIT_FILE_BYTE_LENGTH = 100000;

        internal const string LOG_DIRECTORY = "log";

        internal const string LOG_FILE_PATH = LOG_DIRECTORY + "/debug.log";
        internal const string LOG_FILE_FMT = LOG_DIRECTORY + "/{0}-debug.log";
    }
}
